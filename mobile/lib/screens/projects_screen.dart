import 'package:flutter/material.dart';
import '../api/api_client.dart';
import '../models.dart';
import 'content_screen.dart';
import 'approvals_screen.dart';
import 'schedules_screen.dart';

class ProjectsScreen extends StatefulWidget {
  final Agency agency;
  const ProjectsScreen({super.key, required this.agency});
  @override
  State<ProjectsScreen> createState() => _ProjectsScreenState();
}

class _ProjectsScreenState extends State<ProjectsScreen> {
  late Future<List<Project>> _future;

  @override
  void initState() {
    super.initState();
    _future = _load();
  }

  Future<List<Project>> _load() async {
    final res =
        await ApiClient.get('/api/v1/agencies/${widget.agency.id}/projects');
    return (res['data'] as List).map((j) => Project.fromJson(j)).toList();
  }

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.agency.name),
        actions: [
          IconButton(
            icon: const Icon(Icons.approval_outlined),
            tooltip: 'Approvazioni',
            onPressed: () => Navigator.push(
                context,
                MaterialPageRoute(
                    builder: (_) =>
                        ApprovalsScreen(agency: widget.agency))),
          ),
        ],
      ),
      body: FutureBuilder<List<Project>>(
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
          final projects = snap.data ?? [];
          if (projects.isEmpty) {
            return Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Icon(Icons.folder_open,
                      size: 64,
                      color: cs.onSurfaceVariant.withValues(alpha: 0.5)),
                  const SizedBox(height: 16),
                  Text('Nessun progetto',
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
              itemCount: projects.length,
              itemBuilder: (_, i) => _ProjectCard(
                project: projects[i],
                agency: widget.agency,
              ),
            ),
          );
        },
      ),
    );
  }
}

class _ProjectCard extends StatelessWidget {
  final Project project;
  final Agency agency;
  const _ProjectCard({required this.project, required this.agency});

  @override
  Widget build(BuildContext context) {
    final cs = Theme.of(context).colorScheme;
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: () => Navigator.push(
            context,
            MaterialPageRoute(
                builder: (_) => ContentScreen(
                    agency: agency, project: project))),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  if (project.logoUrl != null && project.logoUrl!.isNotEmpty)
                    ClipRRect(
                      borderRadius: BorderRadius.circular(8),
                      child: Image.network(
                        project.logoUrl!,
                        width: 40,
                        height: 40,
                        fit: BoxFit.cover,
                        errorBuilder: (context, error, stackTrace) => CircleAvatar(
                          backgroundColor: cs.secondaryContainer,
                          child: Text(project.name[0].toUpperCase(),
                              style: TextStyle(
                                  color: cs.onSecondaryContainer)),
                        ),
                      ),
                    )
                  else
                    CircleAvatar(
                      backgroundColor: cs.secondaryContainer,
                      child: Text(
                        project.name.isNotEmpty
                            ? project.name[0].toUpperCase()
                            : '?',
                        style:
                            TextStyle(color: cs.onSecondaryContainer),
                      ),
                    ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(project.name,
                            style: Theme.of(context)
                                .textTheme
                                .titleMedium
                                ?.copyWith(
                                    fontWeight: FontWeight.w600)),
                        if (project.description != null &&
                            project.description!.isNotEmpty)
                          Text(project.description!,
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: Theme.of(context)
                                  .textTheme
                                  .bodySmall
                                  ?.copyWith(
                                      color: cs.onSurfaceVariant)),
                      ],
                    ),
                  ),
                  Icon(Icons.chevron_right,
                      color: cs.onSurfaceVariant),
                ],
              ),
              const SizedBox(height: 12),
              Row(
                children: [
                  _infoChip(Icons.article_outlined,
                      '${project.generatedContentsCount} contenuti', cs),
                  const SizedBox(width: 8),
                  _infoChip(Icons.source_outlined,
                      '${project.contentSourcesCount} fonti', cs),
                  const Spacer(),
                  if (project.websiteUrl != null &&
                      project.websiteUrl!.isNotEmpty)
                    Icon(Icons.language,
                        size: 18, color: cs.onSurfaceVariant),
                ],
              ),
              const SizedBox(height: 8),
              // Quick actions row
              Row(
                children: [
                  ActionChip(
                    avatar: const Icon(Icons.calendar_month, size: 16),
                    label: const Text('Pianificazione'),
                    onPressed: () => Navigator.push(
                      context,
                      MaterialPageRoute(
                        builder: (_) => SchedulesScreen(
                            agency: agency, project: project),
                      ),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _infoChip(IconData icon, String label, ColorScheme cs) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: cs.surfaceContainerHighest,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: cs.onSurfaceVariant),
          const SizedBox(width: 4),
          Text(label,
              style:
                  TextStyle(fontSize: 12, color: cs.onSurfaceVariant)),
        ],
      ),
    );
  }
}
