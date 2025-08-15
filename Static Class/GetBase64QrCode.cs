static partial class StaticMethods
{
	public static string GetBase64QrCode(string uri)
	{
		using var qrGenerator = new QRCoder.QRCodeGenerator();
		var qrCodeData = qrGenerator.CreateQrCode(uri, QRCoder.QRCodeGenerator.ECCLevel.Q);
		using var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
		var qrBytes = qrCode.GetGraphic(20);
		var qrBase64 = Convert.ToBase64String(qrBytes);

		return qrBase64;
	}
}