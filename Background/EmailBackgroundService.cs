using System.Net;
using FeatureRequestDatabase;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;

public class EmailBackgroundService(EmailQueue emailQueue, IServiceProvider services, ILogger<EmailBackgroundService> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Email background service started.");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var emailRequest = await emailQueue.DequeueAsync(stoppingToken); logger.LogInformation("Dequeued successfully.");

				switch (emailRequest.Type)
				{
					case "NewFeatureRequest":
						await NewFeatureRequest(emailRequest);
						break;
					case "Upvote":
						await Upvote(emailRequest);
						break;
					case "Status":
						await StatusChanged(emailRequest);
						break;
					case "CommentAdded":
						await CommentAdded(emailRequest);
						break;
					case "CommentReplied":
						await CommentReplied(emailRequest);
						break;
					case "UpdatePassword":
						await UpdatePassword(emailRequest);
						break;
					default:
						break;
				}
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception ex)
			{
				logger.LogCritical("{error}", ex.ToString());
			}
		}
	}

	private async Task NewFeatureRequest(EmailRequest emailRequest)
	{
		ContextFeatureRequest contextFeatureRequest = new(emailRequest.OrganizationId);

		// The roles of the feature request.
		var featureRequestsRoles = contextFeatureRequest.FeatureRequestInRoles.Where(x => x.FeatureRequestId == emailRequest.FeatureRequest.Id).ToList();

		// Get the services via dependency injection.
		using var scope = services.CreateScope();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		var serviceEmail = scope.ServiceProvider.GetRequiredService<ServiceEmail>();
		var identityContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();

		// The users in the roles of the feature request.
		var usersInRoles = (from user in identityContext.Users
							where user.OrganizationId == emailRequest.OrganizationId
							join userRole in identityContext.UserRoles on user.Id equals userRole.UserId
							join role in identityContext.Roles on userRole.RoleId equals role.Id
							select new
							{
								ApplicationUser = user,
								RoleName = role.Name
							}).ToList();

		bool emailSent = false;
		HashSet<string> usersSent = [];

		// Define local function for sending email
		async Task SendEmailToUser(ApplicationUser user)
		{
			// Check if the user is not the current user and if the user has not already been sent an email.
			if (user.Id != emailRequest.CurrentUser.Id && !usersSent.Contains(user.Id))
			{
				await serviceEmail.SendNewFeatureAddedEmail(user, emailRequest.FeatureRequest, emailRequest.BaseUrl);
				emailSent = true;
				usersSent.Add(user.Id);
			}
		}

		// Notify all superadmins of the organization.
		var superAdmins = await userManager.GetUsersInRoleAsync("Superadmin");
		foreach (var superAdmin in superAdmins)
		{
			if (superAdmin.OrganizationId == emailRequest.OrganizationId)
			{
				await SendEmailToUser(superAdmin);
			}
		}

		// Notify all admins of the organization
		var admins = await userManager.GetUsersInRoleAsync("Admin");
		foreach (var admin in admins)
		{
			if (admin.OrganizationId == emailRequest.OrganizationId)
			{
				await SendEmailToUser(admin);
			}
		}

		// Notify all users in the feature request roles and topic.
		if (emailRequest.FeatureRequest.Topic is null)
		{
			logger.LogError("No topic found in feature request");
			return;
		}

		var userInTopics = contextFeatureRequest.UsersInTopics.Where(x => x.TopicId == emailRequest.FeatureRequest.Topic!.Id).ToList();
		foreach (var role in featureRequestsRoles)
		{
			// The users in the role of the feature request.
			var usersInRole = usersInRoles.Where(x => x.RoleName == role.RoleId);

			// Send an email to all users in the feature request roles.
			foreach (var userRole in usersInRole)
			{
				var user = userRole.ApplicationUser;

				var userInTopic = userInTopics.Any(x => x.UserId == user.Id);

				if (userInTopic)
				{
					await SendEmailToUser(user);
				}
			}
		}

		if (!emailSent)
		{
			logger.LogInformation("No users to send emails to.");
		}
	}

	public async Task Upvote(EmailRequest emailRequest)
	{
		using var scope = services.CreateScope();
		var serviceEmail = scope.ServiceProvider.GetRequiredService<ServiceEmail>();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

		var subscribers = await GetSubscribers(emailRequest.OrganizationId, emailRequest.FeatureRequest.Id, emailRequest.CurrentUser.Id, userManager);

		foreach (var user in subscribers)
		{
			await serviceEmail.SendUpvoteEmail(user.Email, user.Name, emailRequest.FeatureRequest.Heading);
		}
	}

	public async Task StatusChanged(EmailRequest emailRequest)
	{
		using var scope = services.CreateScope();
		var serviceEmail = scope.ServiceProvider.GetRequiredService<ServiceEmail>();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

		var subscribers = await GetSubscribers(emailRequest.OrganizationId, emailRequest.FeatureRequest.Id, emailRequest.CurrentUser.Id, userManager);

		foreach (var user in subscribers)
		{
			await serviceEmail.SendStatusChangedEmail(user.Email, user.Name, emailRequest.FeatureRequest.Heading, GetStatus(emailRequest.FeatureRequest.StatusId, emailRequest.OrganizationId));
		}
	}

	public async Task CommentAdded(EmailRequest emailRequest)
	{
		using var scope = services.CreateScope();
		var serviceEmail = scope.ServiceProvider.GetRequiredService<ServiceEmail>();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

		var subscribers = await GetSubscribers(emailRequest.OrganizationId, emailRequest.FeatureRequest.Id, emailRequest.CurrentUser.Id, userManager);

		foreach (var user in subscribers)
		{
			await serviceEmail.SendCommentAdded(user.Email, user.Name, emailRequest.FeatureRequest.Heading, emailRequest.CommentAdded);
		}
	}

	public async Task CommentReplied(EmailRequest emailRequest)
	{
		using var scope = services.CreateScope();
		var serviceEmail = scope.ServiceProvider.GetRequiredService<ServiceEmail>();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

		var subscribers = await GetSubscribers(emailRequest.OrganizationId, emailRequest.FeatureRequest.Id, emailRequest.CurrentUser.Id, userManager);

		// Check if the user of the parent comment is subscribed and if not, add it so that the user will be notified of a reply.
		ContextFeatureRequest contextFeatureRequest = new(emailRequest.OrganizationId);
		var dbParentComment = contextFeatureRequest.Comments.FirstOrDefault(x => x.Id == emailRequest.ParentCommentId);
		if (dbParentComment is { })
		{
			if (subscribers.All(x => x.UserId != dbParentComment.UserId))
			{
				var parentUser = await userManager.FindByIdAsync(dbParentComment.UserId);
				if (parentUser is { })
				{
					if (parentUser.Email is { })
					{
						User subscriber = new(parentUser.Id, parentUser.Email, parentUser.Name);
						subscribers.Add(subscriber);
					}
				}
			}
		}

		foreach (var user in subscribers)
		{
			await serviceEmail.SendCommentReplied(user.Email, user.Name, emailRequest.FeatureRequest.Heading, emailRequest.CommentReplied);
		}
	}

	public async Task UpdatePassword(EmailRequest emailRequest)
	{
		using var scope = services.CreateScope();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		var serviceEmail = scope.ServiceProvider.GetRequiredService<ServiceEmail>();

		var user = await userManager.FindByEmailAsync(emailRequest.Email);

		if (user is null)
		{
			return;
		}

		var token = await userManager.GeneratePasswordResetTokenAsync(user);

		token = WebUtility.UrlEncode(token);

		await serviceEmail.SendPasswordResetEmail(user.Email ?? "", user.Name, token, emailRequest.BaseUrl);
	}

	private string GetStatus(int statusId, Guid organizationId)
	{
		ContextFeatureRequest contextFeatureRequest = new(organizationId);
		var status = contextFeatureRequest.Statuses.First(x => x.Id == statusId);
		return status.Value;
	}

	private async Task<List<User>> GetSubscribers(Guid organizationId, Guid featureRequestId, string currentUserId, UserManager<ApplicationUser> userManager)
	{
		ContextFeatureRequest contextFeatureRequest = new(organizationId);

		List<User> subscribers = new();

		var dbSubscribers = contextFeatureRequest.Subscribers.Where(x => x.FeatureRequestId == featureRequestId).ToList();

		foreach (var subscriber in dbSubscribers)
		{
			if (subscriber.UserId == currentUserId)
			{
				continue;
			}

			var user = await userManager.FindByIdAsync(subscriber.UserId);

			if (user is { } && user.Email is { })
			{
				subscribers.Add(new User(user.Id, user.Email, user.Name));
			}
		}

		return subscribers;
	}

	private class User
	{
		public string UserId { get; set; }
		public string Email { get; set; }

		public string Name { get; set; }

		public User(string userId, string email, string name)
		{
			UserId = userId;
			Email = email;
			Name = name;
		}
	}
}
