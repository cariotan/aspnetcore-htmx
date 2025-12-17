public class ButtonSpinnerModel
{
	public string IsLoadingModel { get; set; } = null!;
	public string Size { get; set; }
	public string Color { get; set; }

	public ButtonSpinnerModel(
		string isLoadingModel,
		string size = "size-4",
		string color = "text-blue-600"
	)
	{
		IsLoadingModel = isLoadingModel;
		Size = size;
		Color = color;
	}
}