import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../api/api_client.dart';
import '../models.dart';

class SocialConnectorsScreen extends StatefulWidget {
  final Agency agency;
  const SocialConnectorsScreen({super.key, required this.agency});
  @override
  State<SocialConnectorsScreen> createState() => _SocialConnectorsScreenState();
}

class _SocialConnectorsScreenState extends State<SocialConnectorsScreen> {
  late Future<List<Map<String, dynamic>>> _connectorsFuture;

  @override
  void initState() {
    super.initState();
    _connectorsFuture = _loadConnectors();
  }

  Future<List<Map<String, dynamic>>> _loadConnectors() async {
    final res = await ApiClient.get(
        '/api/v1/agencies/${widget.agency.id}/social-connectors');
    return (res['data'] as List).cast<Map<String, dynamic>>();
  }

  IconData _platformIcon(String platform) {
    switch (platform.toLowerCase()) {
      case 'facebook':
        return Icons.facebook;
      case 'instagram':
        return Icons.camera_alt_outlined;
      case 'linkedin':
        return Icons.work_outline;
      case 'twitter':
      case 'x':
        return Icons.alternate_email;
      default:
        return Icons.share_outlined;
    }
  }

  Color _platformColor(String platform) {
    switch (platform.toLowerCase()) {
      case 'facebook':
        return const Color(0xFF1877F2);
      case 'instagram':
        return const Color(0xFFE4405F);
      case 'linkedin':
        return const Color(0xFF0A66C2);
      case 'twitter':
      case 'x':
        return const Color(0xFF1DA1F2);
      default:
        return Colors.grey;
    }
  }

  String _formatDate(String? dateStr) {
    if (dateStr == null || dateStr.isEmpty) return '';
    try {
      final date = DateTime.parse(dateStr);
      return '${date.day.toString().padLeft(2, '0')}/${date.month.toString().padLeft(2, '0')}/${date.year}';
    } catch (_) {
      return dateStr;
    }
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(
        title: Text('Social - ${widget.agency.name}'),
      ),
      body: RefreshIndicator(
        onRefresh: () async {
          setState(() {
            _connectorsFuture = _loadConnectors();
          });
        },
        child: FutureBuilder<List<Map<String, dynamic>>>(
          future: _connectorsFuture,
          builder: (context, snap) {
            if (snap.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }
            if (snap.hasError) {
              return ListView(
                padding: const EdgeInsets.all(12),
                children: [
                  const SizedBox(height: 80),
                  Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      const Icon(Icons.error_outline,
                          size: 48, color: Colors.red),
                      const SizedBox(height: 8),
                      Text('Errore: ${snap.error}',
                          textAlign: TextAlign.center),
                      const SizedBox(height: 8),
                      FilledButton.tonal(
                        onPressed: () =>
                            setState(() => _connectorsFuture = _loadConnectors()),
                        child: const Text('Riprova'),
                      ),
                    ],
                  ),
                ],
              );
            }
            final connectors = snap.data ?? [];
            return ListView(
              padding: const EdgeInsets.all(12),
              children: [
                // Info banner
                Card(
                  color: cs.primaryContainer,
                  child: Padding(
                    padding: const EdgeInsets.all(12),
                    child: Row(
                      children: [
                        Icon(Icons.info_outline,
                            color: cs.onPrimaryContainer, size: 20),
                        const SizedBox(width: 10),
                        Expanded(
                          child: Text(
                            'I connettori social si configurano dalla webapp. Qui puoi visualizzare lo stato.',
                            style: TextStyle(
                              fontSize: 13,
                              color: cs.onPrimaryContainer,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 12),
                // Connectors list or empty state
                if (connectors.isEmpty)
                  Padding(
                    padding: const EdgeInsets.symmetric(vertical: 48),
                    child: Center(
                      child: Column(
                        children: [
                          Icon(Icons.share_outlined,
                              size: 48,
                              color: cs.onSurfaceVariant
                                  .withValues(alpha: 0.5)),
                          const SizedBox(height: 12),
                          Text(
                            'Nessun connettore social configurato.\nConfigura i connettori dalla webapp.',
                            textAlign: TextAlign.center,
                            style: TextStyle(color: cs.onSurfaceVariant),
                          ),
                        ],
                      ),
                    ),
                  )
                else
                  ...connectors.map((c) => _buildConnectorCard(c, cs)),
              ],
            );
          },
        ),
      ),
    );
  }

  Widget _buildConnectorCard(Map<String, dynamic> connector, ColorScheme cs) {
    final platform = connector['platform'] as String? ?? '';
    final accountName = connector['accountName'] as String? ?? '';
    final profileImageUrl = connector['profileImageUrl'] as String?;
    final isActive = connector['isActive'] == true;
    final connectedAt = connector['connectedAt'] as String?;
    final platformColor = _platformColor(platform);

    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Row(
          children: [
            // Profile image or platform icon
            if (profileImageUrl != null && profileImageUrl.isNotEmpty)
              ClipRRect(
                borderRadius: BorderRadius.circular(24),
                child: CachedNetworkImage(
                  imageUrl: profileImageUrl,
                  width: 48,
                  height: 48,
                  fit: BoxFit.cover,
                  placeholder: (context, url) => CircleAvatar(
                    radius: 24,
                    backgroundColor: platformColor.withValues(alpha: 0.12),
                    child: Icon(_platformIcon(platform),
                        color: platformColor, size: 24),
                  ),
                  errorWidget: (context, url, error) => CircleAvatar(
                    radius: 24,
                    backgroundColor: platformColor.withValues(alpha: 0.12),
                    child: Icon(_platformIcon(platform),
                        color: platformColor, size: 24),
                  ),
                ),
              )
            else
              CircleAvatar(
                radius: 24,
                backgroundColor: platformColor.withValues(alpha: 0.12),
                child: Icon(_platformIcon(platform),
                    color: platformColor, size: 24),
              ),
            const SizedBox(width: 14),
            // Info
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    accountName.isNotEmpty ? accountName : platform,
                    style: Theme.of(context)
                        .textTheme
                        .titleSmall
                        ?.copyWith(fontWeight: FontWeight.w600),
                  ),
                  const SizedBox(height: 4),
                  Row(
                    children: [
                      Icon(_platformIcon(platform),
                          size: 14, color: platformColor),
                      const SizedBox(width: 4),
                      Text(
                        platform,
                        style: TextStyle(
                            fontSize: 12, color: cs.onSurfaceVariant),
                      ),
                    ],
                  ),
                  if (connectedAt != null && connectedAt.isNotEmpty) ...[
                    const SizedBox(height: 4),
                    Row(
                      children: [
                        Icon(Icons.calendar_today,
                            size: 12, color: cs.onSurfaceVariant),
                        const SizedBox(width: 4),
                        Text(
                          'Connesso il ${_formatDate(connectedAt)}',
                          style: TextStyle(
                              fontSize: 11, color: cs.onSurfaceVariant),
                        ),
                      ],
                    ),
                  ],
                ],
              ),
            ),
            // Active/Inactive badge
            Container(
              padding:
                  const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
              decoration: BoxDecoration(
                color: (isActive ? Colors.green : Colors.grey)
                    .withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(
                    isActive ? Icons.check_circle : Icons.cancel_outlined,
                    size: 14,
                    color: isActive ? Colors.green : Colors.grey,
                  ),
                  const SizedBox(width: 4),
                  Text(
                    isActive ? 'Attivo' : 'Inattivo',
                    style: TextStyle(
                      fontSize: 11,
                      fontWeight: FontWeight.w600,
                      color: isActive ? Colors.green : Colors.grey,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
