using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

public class HtmxController : Controller
{
	public void Hx(string target, bool outerHTML, bool? sameSelectAsTarget = null)
	{
		Hx(target, outerHTML ? "outerHTML" : "innerHTML", sameSelectAsTarget == true ? target : "");
	}

	public void Hx(string target, bool outerHTML, string select)
	{
		Hx(target, outerHTML ? "outerHTML" : "innerHTML", select);
	}

	public void Hx(string target = "this", string swap = "outerHTML", string select = "")
	{
		if (Response.Headers.ContainsKey("hx-retarget"))
		{
			Response.Headers["hx-retarget"] = target;
		}
		else
		{
			Response.Headers.Append("hx-retarget", target);
		}

		if (Response.Headers.ContainsKey("hx-reswap"))
		{
			Response.Headers["hx-reswap"] = swap;
		}
		else
		{
			Response.Headers.Append("hx-reswap", swap);
		}

		if (!string.IsNullOrWhiteSpace(select))
		{
			if (Response.Headers.ContainsKey("hx-reselect"))
			{
				Response.Headers["hx-reselect"] = target;
			}
			else
			{
				Response.Headers.Append("hx-reselect", target);
			}
		}
	}

	public void HxTrigger(string trigger, object? value = null)
	{
		value ??= "";

		// Serialize the key-value pair into JSON
		Dictionary<string, object> payload = new()
		{
			[trigger] = value
		};

		string triggerValue = JsonSerializer.Serialize(payload);

		if (Response.Headers.ContainsKey("hx-trigger"))
		{
			payload = JsonSerializer.Deserialize<Dictionary<string, object>>(Response.Headers["hx-trigger"]!)!;

			payload.Add(trigger, value);

			var json = JsonSerializer.Serialize(payload);

			Response.Headers["hx-trigger"] = json;
		}
		else
		{
			Response.Headers.Append("hx-trigger", triggerValue);
		}
	}

	public void HxRedirect(string localPath)
	{
		if (Response.Headers.ContainsKey("hx-redirect"))
		{
			Response.Headers["hx-redirect"] = localPath;
		}
		else
		{
			Response.Headers.Append("hx-redirect", localPath);
		}
	}

	public void HxNone()
	{
		if (Response.Headers.ContainsKey("hx-reswap"))
		{
			Response.Headers["hx-reswap"] = "none";
		}
		else
		{
			Response.Headers.Append("hx-reswap", "none");
		}
	}

	public IActionResult PartialView(Partials partials, object model)
	{
		return base.PartialView(partials.ToString(), model);
	}

	public IActionResult PartialView(Partials partials)
	{
		return base.PartialView(partials.ToString());
	}
}
