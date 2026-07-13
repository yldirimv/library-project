import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

class ApiClient {
  // Bilgisayarın yerel ağ IP'si — API buradan dinliyor
  static const String baseUrl = 'http://YOUR_PC_LOCAL_IP:5193/api';

  // Cihazda saklı token'ı oku (yoksa null)
  static Future<String?> _getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString('jwt_token');
  }

  // Ortak başlıklar: JSON + (varsa) Bearer bileti
  static Future<Map<String, String>> _headers() async {
    final token = await _getToken();
    return {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
  }

  static Future<http.Response> get(String path) async =>
      http.get(Uri.parse('$baseUrl$path'), headers: await _headers());

  static Future<http.Response> post(String path, {Object? body}) async =>
      http.post(
        Uri.parse('$baseUrl$path'),
        headers: await _headers(),
        body: body != null ? jsonEncode(body) : null,
      );
}