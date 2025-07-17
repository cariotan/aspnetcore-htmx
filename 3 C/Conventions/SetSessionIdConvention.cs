using Microsoft.AspNetCore.Mvc.ApplicationModels;

public class SetSessionIdConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            var controllerType = controller.ControllerType.AsType();

            if (typeof(IHasSessionId).IsAssignableFrom(controllerType))
            {
                controller.Filters.Add(new SetSessionAttribute());
            }
        }
    }
}