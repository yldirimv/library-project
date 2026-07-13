class Loan {
  final int id;
  final String title;
  final String author;
  final DateTime dueDate;
  final bool returned;
  final bool overdue;

  Loan({required this.id, required this.title, required this.author,
    required this.dueDate, required this.returned, required this.overdue});

  factory Loan.fromJson(Map<String, dynamic> json) => Loan(
    id: json['id'],
    title: json['title'],
    author: json['author'],
    dueDate: DateTime.parse(json['dueDate']),
    returned: json['returned'],
    overdue: json['overdue'],
  );
}