using System.Runtime.Serialization;

namespace Divoom.Api.Models;

[DataContract]
public class GetDialInfoRequest : BasicCommand
{
	public GetDialInfoRequest() : base("Channel/GetClockInfo")
	{
	}
}
