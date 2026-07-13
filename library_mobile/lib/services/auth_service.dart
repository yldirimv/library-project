import 'dart:convert';
import 'package:shared_preferences/shared_preferences.dart';
import 'api_client.dart';

class AuthService {
  /// Başarıda null, hatada kullanıcıya gösterilecek mesaj döner
  static Future<String?> login(String email, String password) async {
    try {
      final response = await ApiClient.post('/auth/login',
          body: {'email': email, 'password': password});

      final data = jsonDecode(response.body);

      if (response.statusCode == 200) {
        final prefs = await SharedPreferences.getInstance();
        await prefs.setString('jwt_token', data['token']);
        await prefs.setString('full_name', data['fullName']);
        return null; // başarı
      }
      return data['message'] ?? 'Giriş başarısız';
    } catch (e) {
      return 'Sunucuya ulaşılamadı. Aynı Wi-Fi ağında mısınız?';
    }
  }

  static Future<bool> isLoggedIn() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('jwt_token') != null;
  }

  static Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.clear();
  }
}