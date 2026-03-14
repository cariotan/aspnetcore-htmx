using Microsoft.AspNetCore.Mvc;

public interface IViewRenderService
{
 string RenderToString(ActionContext actionContext, string viewName, object model);
}