using Microsoft.AspNetCore.Mvc;

public class HtmxController : Controller
{
	public void Hx(string target = "this", string swap = "outerHTML", string select = "")
	{
		Console.WriteLine("Ensure Ids include #");
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

	public void HxTrigger(string trigger)
	{
		if (Response.Headers.ContainsKey("hx-trigger"))
		{
			Response.Headers["hx-trigger"] += ", " + trigger;
		}
		else
		{
			Response.Headers.Append("hx-trigger", trigger);
		}

		Console.WriteLine("Don't forget to listen for the event from:body");
	}

	public void HxTrigger(string trigger, string value)
	{
		string triggerValue = $"{{\"{trigger}\":\"{value}\"}}";
		if (Response.Headers.ContainsKey("hx-trigger"))
		{
			Response.Headers["hx-trigger"] += ", " + triggerValue;
		}
		else
		{
			Response.Headers.Append("hx-trigger", triggerValue);
		}

		Console.WriteLine("Don't forget to listen for the event from:body");
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
