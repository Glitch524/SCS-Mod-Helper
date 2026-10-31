using SCS_Mod_Helper.Utils;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace SCS_Mod_Helper.HttpService; 

public class UserInfo {
	public string? DisplayName { get; set; }
	public string? AvatarUrl { get; set; }
}

public interface IIOService {
	Task<UserInfo?> GetAvatarAsync();
}