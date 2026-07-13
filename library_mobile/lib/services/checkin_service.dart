import 'dart:convert';
import 'api_client.dart';

class CheckInService {
  /// action: CheckIn | BreakStart | BreakEnd | CheckOut
  /// Dönen: (başarı, mesaj)
  static Future<(bool, String)> process(String token, String action) async {
    try {
      final response = await ApiClient.post('/checkin',
          body: {'token': token, 'action': action});

      final data = jsonDecode(response.body);
      return (response.statusCode == 200, data['message'] as String);
    } catch (e) {
      return (false, 'Sunucuya ulaşılamadı');
    }
  }
}