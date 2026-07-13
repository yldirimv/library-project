class Reservation {
  final int id;
  final String seatNumber;
  final DateTime start;
  final DateTime end;
  final String status;

  Reservation({required this.id, required this.seatNumber,
    required this.start, required this.end, required this.status});

  factory Reservation.fromJson(Map<String, dynamic> json) => Reservation(
    id: json['id'],
    seatNumber: json['seatNumber'],
    start: DateTime.parse(json['start']),
    end: DateTime.parse(json['end']),
    status: json['status'],
  );
}