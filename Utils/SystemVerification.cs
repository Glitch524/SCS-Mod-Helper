using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using SCS_Mod_Helper.Language;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows;

namespace SCS_Mod_Helper.Utils; 
public class SystemVerification {
	public static string? GetMachineGuid() {
		const string regPath = @"SOFTWARE\Microsoft\Cryptography";
		const string regValue = "MachineGuid";
		try {
			using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
			using var key = hklm.OpenSubKey(regPath);
			if (key != null) {
				var value = key.GetValue(regValue);
				if (value != null)
					return value.ToString()?.ToUpper();
			}
		} catch { }
		return null;
	}
	public static string? GetSmbiosUuid() {
		try {
			using var searcher = new ManagementObjectSearcher(
				"SELECT UUID FROM Win32_ComputerSystemProduct");
			foreach (ManagementObject obj in searcher.Get()) {
				var uuid = obj["UUID"]?.ToString();
				if (!string.IsNullOrEmpty(uuid) &&
					uuid != "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")  // 排除无效值
					return uuid.ToUpper();
			}
		} catch { }
		return null;
	}

	private static string CPTCheck => "Data Source=" + Paths.StorageDBPath();

	public static async Task StorageAsync() {
		string dbPath = Paths.StorageDBPath();
		try {
			if (File.Exists(dbPath)) {
				await using var conn = new SqliteConnection(CPTCheck);
				await conn.OpenAsync();

				var cmd = conn.CreateCommand();
				cmd.CommandText = $"select * from Store where Key=$key";
				cmd.Parameters.AddWithValue("$key", "lastRun");

				await using var reader = await cmd.ExecuteReaderAsync();
				if (await reader.ReadAsync()) {
					string? guid = reader.IsDBNull(1) ? null : reader.GetString(1);
					string? currentGuid = GetMachineGuid();
					if (currentGuid != guid) {
						_ = Application.Current.Dispatcher.BeginInvoke(() => {
							MessageBox.Show(Resource.GetString("MessageCheck"));
						});
						var updateCmd = conn.CreateCommand();
						cmd.CommandText = @$"update Store 
							set MachineGuid = $guid 
							where Key = $key";
						cmd.Parameters.AddWithValue("$guid", GetMachineGuid());
						cmd.Parameters.AddWithValue("$key", "lastRun");
						await updateCmd.ExecuteNonQueryAsync();
					}
				}
			} else {
				await using var conn = new SqliteConnection(CPTCheck);
				await conn.OpenAsync();
				var cmd = conn.CreateCommand();
				cmd.CommandText = "CREATE TABLE Store (Key TEXT, MachineGuid TEXT)";
				await cmd.ExecuteNonQueryAsync();
				cmd.CommandText = @$"Insert into Store (Key, MachineGuid)
							values($key, $guid)";
				cmd.Parameters.AddWithValue("$key", "lastRun");
				cmd.Parameters.AddWithValue("$guid", GetMachineGuid());
				await cmd.ExecuteNonQueryAsync();
			}
		} catch (Exception ex) {
			Debug.WriteLine($"Storage verification failed: {ex.Message}");
		}
	}
}