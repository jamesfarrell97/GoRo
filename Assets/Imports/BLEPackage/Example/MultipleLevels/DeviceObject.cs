public class DeviceObject
{
	public string Name;
	public string Address;
	public string Status;

	public DeviceObject ()
	{
		Name = "";
		Address = "";
        Status = "";
	}

	public DeviceObject (string address, string name, string status = "Disconnected")
	{
		Name = name;
		Address = address;
        Status = status;
	}
}
