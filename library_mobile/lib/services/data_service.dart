import 'dart:convert';
import '../models/reservation.dart';
import '../models/loan.dart';
import '../models/announcement.dart';
import 'api_client.dart';

class DataService {
  static Future<List<Reservation>> getReservations() async {
    final response = await ApiClient.get('/reservations/mine');
    final list = jsonDecode(response.body) as List;
    return list.map((e) => Reservation.fromJson(e)).toList();
  }

  static Future<List<Loan>> getLoans() async {
    final response = await ApiClient.get('/loans/mine');
    final list = jsonDecode(response.body) as List;
    return list.map((e) => Loan.fromJson(e)).toList();
  }

  static Future<List<Announcement>> getAnnouncements() async {
    final response = await ApiClient.get('/announcements');
    final list = jsonDecode(response.body) as List;
    return list.map((e) => Announcement.fromJson(e)).toList();
  }
  static Future<(bool, String)> reportNoise() async {
  try {
    final response = await ApiClient.post('/noise');
    final data = jsonDecode(response.body);
    return (response.statusCode == 200, data['message'] as String);
  } catch (e) {
    return (false, 'Sunucuya ulaşılamadı');
  }
}
}