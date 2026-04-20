import 'package:flutter/material.dart';
import 'package:cached_network_image/cached_network_image.dart';
import 'package:intl/intl.dart';
import '../api/api_client.dart';
import '../models.dart';
import 'generate_content_screen.dart';

class ContentScreen extends StatefulWidget {
  final Agency agency;
  final Project? project;
  const ContentScreen({super.key, required this.agency, this.project});
  @override
  State<ContentScreen> createState() => _ContentScreenState();
}

class _ContentScreenState extends State<ContentScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  late Future<List<GeneratedContent>> _future;

  // contentType values (backend enum): 1=BlogPost, 2=SocialPost, 3=Newsletter,
  // 4=Report, 5=Video, 6=Carousel, 7=Infographic
  static const _tabs = [
    _TabDef(null, 'Tutti', Icons.dashboard_outlined),
    _TabDef(2, 'Social', Icons.share_outlined),
    _TabDef(1, 'Blog', Icons.article_outlined),
    _TabDef(3, 'Newsletter', Icons.email_outlined),
  ];

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: _tabs.length, vsync: this);
    _future = _load();
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Future<List<GeneratedContent>> _load() async {
    var path = '/api/v1/agencies/${widget.agency.id}/content';
    if (widget.project != null) {
      path += '?projectId=${widget.project!.id}';
    }
    final res = await ApiClient.get(path);
    return (res['data'] as List)
        .map((j) => GeneratedContent.fromJson(j))
        .toList();
  }

  Future<void> _approve(String id) async {
    final messenger = ScaffoldMessenger.of(context);
    try {
      await ApiClient.post(
          '/api/v1/agencies/${widget.agency.id}/content/$id/approve');
      messenger
          .showSnackBar(const SnackBar(content: Text('Contenuto approvato')));
      if (mounted) setState(() => _future = _load());
    } catch (e) {
      messenger.showSnackBar(SnackBar(content: Text('Errore: $e')));
    }
  }

  Future<void> _reject(String id) async {
    final messenger = ScaffoldMessenger.of(context);
    try {
      await ApiClient.post(
          '/api/v1/agencies/${widget.agency.id}/content/$id/reject');
      messenger
          .showSnackBar(const SnackBar(content: Text('Contenuto rifiutato')));
      if (mounted) setState(() => _future = _load());
    } catch (e) {
      messenger.showSnackBar(SnackBar(content: Text('Errore: $e')));
    }
  }

  Color _statusColor(int s) {
    switch (s) {
      case 1:
        return Colors.grey;
      case 2:
        return Colors.orange;
      case 3:
        return Colors.blue;
      case 4:
        return Colors.green;
      case 5:
        return Colors.red;
      default:
        return Colors.grey;
    }
  }

  String _statusLabel(int s) {
    switch (s) {
      case 1:
        return 'Bozza';
      case 2:
        return 'In review';
      case 3:
        return 'Approvato';
      case 4:
        return 'Pubblicato';
      case 5:
        return 'Rifiutato';
      default:
        return '?';
    }
  }

  IconData _statusIcon(int s) {
    switch (s) {
      case 1:
        return Icons.edit_note;
      case 2:
        return Icons.rate_review;
      case 3:
        return Icons.check_circle_outline;
      case 4:
        return Icons.public;
      case 5:
        return Icons.cancel_outlined;
      default:
        return Icons.help_outline;
    }
  }

  String _contentTypeLabel(int? t) {
    switch (t) {
      case 1:
        return 'Blog';
      case 2:
        return 'Social';
      case 3:
        return 'Newsletter';
      case 4:
        return 'Report';
      case 5:
        return 'Video';
      case 6:
        return 'Carousel';
      case 7:
        return 'Infografica';
      default:
        return '';
    }
  }

  IconData _contentTypeIcon(int? t) {
    switch (t) {
      case 1:
        return Icons.article;
      case 2:
        return Icons.share;
      case 3:
        return Icons.email;
      case 4:
        return Icons.assessment;
      case 5:
        return Icons.videocam;
      case 6:
        return Icons.view_carousel;
      case 7:
        return Icons.insights;
      default:
        return Icons.description;
    }
  }

  Color _contentTypeColor(int? t) {
    switch (t) {
      case 1:
        return Colors.teal;
      case 2:
        return Colors.indigo;
      case 3:
        return Colors.deepPurple;
      case 4:
        return Colors.brown;
      case 5:
        return Colors.red;
      case 6:
        return Colors.orange;
      case 7:
        return Colors.cyan;
      default:
        return Colors.grey;
    }
  }

  @override
  Widget build(BuildContext context) {
    final title =
        widget.project != null ? widget.project!.name : widget.agency.name;
    return Scaffold(
      appBar: AppBar(
        title: Text(title),
        bottom: TabBar(
          controller: _tabController,
          isScrollable: false,
          tabs: _tabs
              .map((t) => Tab(icon: Icon(t.icon, size: 18), text: t.label))
              .toList(),
        ),
      ),
      floatingActionButton: widget.project != null
          ? FloatingActionButton.extended(
              onPressed: () async {
                await Navigator.push(
                    context,
                    MaterialPageRoute(
                        builder: (_) => GenerateContentScreen(
                            agency: widget.agency,
                            project: widget.project!)));
                setState(() => _future = _load());
              },
              icon: const Icon(Icons.auto_awesome),
              label: const Text('Genera'),
            )
          : null,
      body: FutureBuilder<List<GeneratedContent>>(
        future: _future,
        builder: (context, snap) {
          if (snap.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snap.hasError) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Icon(Icons.error_outline,
                      size: 48, color: Colors.red),
                  const SizedBox(height: 16),
                  Text('Errore: ${snap.error}',
                      textAlign: TextAlign.center),
                  const SizedBox(height: 16),
                  FilledButton.tonal(
                    onPressed: () => setState(() => _future = _load()),
                    child: const Text('Riprova'),
                  ),
                ],
              ),
            );
          }
          final allItems = snap.data ?? [];
          return TabBarView(
            controller: _tabController,
            children: _tabs.map((tab) {
              final items = tab.typeFilter == null
                  ? allItems
                  : allItems
                      .where((c) => c.contentType == tab.typeFilter)
                      .toList();
              return _buildContentList(items);
            }).toList(),
          );
        },
      ),
    );
  }

  Widget _buildContentList(List<GeneratedContent> items) {
    final cs = Theme.of(context).colorScheme;
    if (items.isEmpty) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.article_outlined,
                size: 64,
                color: cs.onSurfaceVariant.withValues(alpha: 0.5)),
            const SizedBox(height: 16),
            Text('Nessun contenuto',
                style: Theme.of(context)
                    .textTheme
                    .titleMedium
                    ?.copyWith(color: cs.onSurfaceVariant)),
          ],
        ),
      );
    }
    return RefreshIndicator(
      onRefresh: () async => setState(() => _future = _load()),
      child: ListView.builder(
        padding: const EdgeInsets.symmetric(vertical: 8),
        itemCount: items.length,
        itemBuilder: (_, i) {
          final c = items[i];
          return _ContentCard(
            content: c,
            statusColor: _statusColor(c.status),
            statusLabel: _statusLabel(c.status),
            statusIcon: _statusIcon(c.status),
            contentTypeLabel: _contentTypeLabel(c.contentType),
            contentTypeIcon: _contentTypeIcon(c.contentType),
            contentTypeColor: _contentTypeColor(c.contentType),
            onApprove:
                (c.status == 2 || c.status == 5) ? () => _approve(c.id) : null,
            onReject:
                (c.status == 2 || c.status == 5) ? () => _reject(c.id) : null,
          );
        },
      ),
    );
  }
}

class _TabDef {
  final int? typeFilter;
  final String label;
  final IconData icon;
  const _TabDef(this.typeFilter, this.label, this.icon);
}

class _ContentCard extends StatelessWidget {
  final GeneratedContent content;
  final Color statusColor;
  final String statusLabel;
  final IconData statusIcon;
  final String contentTypeLabel;
  final IconData contentTypeIcon;
  final Color contentTypeColor;
  final VoidCallback? onApprove;
  final VoidCallback? onReject;

  const _ContentCard({
    required this.content,
    required this.statusColor,
    required this.statusLabel,
    required this.statusIcon,
    required this.contentTypeLabel,
    required this.contentTypeIcon,
    required this.contentTypeColor,
    this.onApprove,
    this.onReject,
  });

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
      clipBehavior: Clip.antiAlias,
      child: ExpansionTile(
        leading: Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: statusColor.withValues(alpha: 0.15),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Icon(statusIcon, color: statusColor, size: 20),
        ),
        title: Text(content.title,
            maxLines: 2, overflow: TextOverflow.ellipsis),
        subtitle: Row(
          children: [
            Container(
              padding:
                  const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
              decoration: BoxDecoration(
                color: statusColor.withValues(alpha: 0.15),
                borderRadius: BorderRadius.circular(12),
              ),
              child: Text(statusLabel,
                  style: TextStyle(
                      fontSize: 11,
                      color: statusColor,
                      fontWeight: FontWeight.w600)),
            ),
            const SizedBox(width: 6),
            if (contentTypeLabel.isNotEmpty)
              Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                decoration: BoxDecoration(
                  color: contentTypeColor.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(contentTypeIcon, size: 12, color: contentTypeColor),
                    const SizedBox(width: 3),
                    Text(contentTypeLabel,
                        style: TextStyle(
                            fontSize: 10,
                            color: contentTypeColor,
                            fontWeight: FontWeight.w600)),
                  ],
                ),
              ),
            const SizedBox(width: 6),
            Flexible(
              child: Text(
                  'Score ${content.overallScore.toStringAsFixed(1)}/10',
                  overflow: TextOverflow.ellipsis,
                  style: Theme.of(context)
                      .textTheme
                      .bodySmall
                      ?.copyWith(color: cs.onSurfaceVariant)),
            ),
            if (content.isScheduled && content.scheduledAt != null) ...[
              const SizedBox(width: 6),
              Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                decoration: BoxDecoration(
                  color: Colors.blue.withValues(alpha: 0.12),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(Icons.calendar_month,
                        size: 11, color: Colors.blue),
                    const SizedBox(width: 3),
                    Text(
                      DateFormat('d MMM HH:mm', 'it_IT')
                          .format(content.scheduledAt!.toLocal()),
                      style: const TextStyle(
                          fontSize: 10,
                          color: Colors.blue,
                          fontWeight: FontWeight.w600),
                    ),
                  ],
                ),
              ),
            ],
          ],
        ),
        children: [
          if (content.imageUrl != null)
            Padding(
              padding:
                  const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
              child: ClipRRect(
                borderRadius: BorderRadius.circular(12),
                child: CachedNetworkImage(
                  imageUrl: content.imageUrl!.replaceAll(
                      'https://api.wepostai.com',
                      'https://wepostai-api.azurewebsites.net'),
                  height: 200,
                  width: double.infinity,
                  fit: BoxFit.cover,
                  placeholder: (context, url) => Container(
                    height: 200,
                    color: cs.surfaceContainerHighest,
                    child: const Center(child: CircularProgressIndicator()),
                  ),
                  errorWidget: (context, url, error) => Container(
                    height: 100,
                    color: cs.surfaceContainerHighest,
                    child: const Icon(Icons.broken_image),
                  ),
                ),
              ),
            ),
          Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(content.body,
                    style: Theme.of(context).textTheme.bodyMedium),
                if (onApprove != null || onReject != null) ...[
                  const SizedBox(height: 16),
                  Row(
                    children: [
                      if (onApprove != null)
                        FilledButton.icon(
                          icon: const Icon(Icons.check, size: 18),
                          label: const Text('Approva'),
                          onPressed: onApprove,
                        ),
                      if (onApprove != null && onReject != null)
                        const SizedBox(width: 8),
                      if (onReject != null)
                        OutlinedButton.icon(
                          icon: Icon(Icons.close,
                              size: 18, color: cs.error),
                          label: Text('Rifiuta',
                              style: TextStyle(color: cs.error)),
                          onPressed: onReject,
                        ),
                    ],
                  ),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }
}
