class Announcement {
  final int id;
  final String title;
  final String content;
  final DateTime date;

  Announcement({required this.id, required this.title,
    required this.content, required this.date});

  factory Announcement.fromJson(Map<String, dynamic> json) => Announcement(
    id: json['id'],
    title: json['title'],
    content: json['content'],
    date: DateTime.parse(json['date']),
  );
}