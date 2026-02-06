using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public class HtmxController : Controller
{
	protected void Hx(string target, bool outerHTML, bool? sameSelectAsTarget = null)
	{
		Hx(target, outerHTML ? "outerHTML" : "innerHTML", sameSelectAsTarget == true ? target : "");
	}

	protected void Hx(string target, bool outerHTML, string select)
	{
		Hx(target, outerHTML ? "outerHTML" : "innerHTML", select);
	}

	protected void Hx(string target = "this", string swap = "outerHTML", string select = "")
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
				Response.Headers["hx-reselect"] = select;
			}
			else
			{
				Response.Headers.Append("hx-reselect", select);
			}
		}
	}

	protected void HxTrigger(string trigger, object? value = null)
	{
		value ??= "";

		// Serialize the key-value pair into JSON
		Dictionary<string, object> payload = new()
		{
			[trigger] = value
		};

		string triggerValue = JsonConvert.SerializeObject(payload);

		if (Response.Headers.ContainsKey("hx-trigger"))
		{
			payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(Response.Headers["hx-trigger"]!)!;

			payload.Add(trigger, value);

			var json = JsonConvert.SerializeObject(payload);

			Response.Headers["hx-trigger"] = json;
		}
		else
		{
			Response.Headers.Append("hx-trigger", triggerValue);
		}
	}

	protected IActionResult HxRedirect(string localPath)
	{
		if (Response.Headers.ContainsKey("hx-redirect"))
		{
			Response.Headers["hx-redirect"] = localPath;
		}
		else
		{
			Response.Headers.Append("hx-redirect", localPath);
		}

		return Ok();
	}

	protected void HxNone()
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

	protected IActionResult HxShowErrorModal(string error)
	{
		HxTrigger("show_error_modal", new
		{
			error = error
		});

		ViewData["show_error_modal"] = error;

		return Ok();
	}

	protected bool IsHTMXRequest()
	{
		return Request.Headers.ContainsKey("HX-Request");
	}
}
