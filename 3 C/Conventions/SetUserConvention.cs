using Microsoft.AspNetCore.Mvc.ApplicationModels;

public class SetUserConvention : IApplicationModelConvention
{
	public void Apply(ApplicationModel application)
	{
		foreach (var controller in application.Controllers)
		{
			if (typeof(IHasUser).IsAssignableFrom(controller.ControllerType))
			{
				controller.Filters.Add(new SetUserAttribute());
			}
		}
	}
}