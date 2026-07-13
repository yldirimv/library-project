import 'package:flutter/material.dart';
import '../models/loan.dart';
import '../services/data_service.dart';

class LoansScreen extends StatelessWidget {
  const LoansScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Kitaplarım')),
      body: FutureBuilder<List<Loan>>(
        future: DataService.getLoans(),
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return const Center(child: Text('Veri alınamadı'));
          }
          final items = snapshot.data ?? [];
          if (items.isEmpty) {
            return const Center(child: Text('Ödünç aldığınız kitap yok'));
          }
          return ListView.builder(
            itemCount: items.length,
            itemBuilder: (context, i) {
              final l = items[i];
              return Card(
                margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                child: ListTile(
                  leading: const Icon(Icons.book, size: 32),
                  title: Text(l.title),
                  subtitle: Text('${l.author}\nSon teslim: '
                      '${l.dueDate.day}.${l.dueDate.month}.${l.dueDate.year}'),
                  isThreeLine: true,
                  trailing: l.returned
                      ? const Chip(label: Text('İade Edildi', style: TextStyle(fontSize: 12)))
                      : Chip(
                          label: Text(l.overdue ? 'Gecikmiş!' : 'Üzerimde',
                              style: const TextStyle(color: Colors.white, fontSize: 12)),
                          backgroundColor: l.overdue ? Colors.red : Colors.green,
                        ),
                ),
              );
            },
          );
        },
      ),
    );
  }
}