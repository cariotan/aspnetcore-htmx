using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public class ViewRenderService(
 IRazorViewEngine viewEngine,
 ITempDataProvider tempDataProvider) : IViewRenderService
{
 public string RenderToString(ActionContext actionContext, string viewName, object model)
 {
  // Now it uses the REAL context, so it knows to look in Areas/V1/Views/Home/
  var viewResult = viewEngine.FindView(actionContext, viewName, false);

  if (!viewResult.Success)
  {
   // Fallback just in case you pass a full path like "~/Views/Shared/..."
   viewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: false);
  }

  if (!viewResult.Success)
  {
   var searched = string.Join("\n", viewResult.SearchedLocations);
   throw new InvalidOperationException($"Could not find view '{viewName}'.\nSearched locations:\n{searched}");
  }

  var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
  {
   Model = model
  };

  using var sw = new StringWriter();
  var viewContext = new ViewContext(
   actionContext,
   viewResult.View,
   viewDictionary,
   new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
   sw,
   new HtmlHelperOptions()
  );

  viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
  return sw.ToString();
 }
}