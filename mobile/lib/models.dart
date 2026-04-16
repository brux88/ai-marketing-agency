class Agency {
  final String id;
  final String name;
  final String productName;
  final String? description;
  final int contentSourcesCount;
  final int generatedContentsCount;

  Agency({
    required this.id,
    required this.name,
    required this.productName,
    this.description,
    required this.contentSourcesCount,
    required this.generatedContentsCount,
  });

  factory Agency.fromJson(Map<String, dynamic> j) => Agency(
        id: j['id'],
        name: j['name'],
        productName: j['productName'] ?? '',
        description: j['description'],
        contentSourcesCount: j['contentSourcesCount'] ?? 0,
        generatedContentsCount: j['generatedContentsCount'] ?? 0,
      );
}

class GeneratedContent {
  final String id;
  final String title;
  final String body;
  final int status;
  final double overallScore;
  final String? imageUrl;
  final DateTime createdAt;
  final String? projectName;
  final int? contentType;

  GeneratedContent({
    required this.id,
    required this.title,
    required this.body,
    required this.status,
    required this.overallScore,
    this.imageUrl,
    required this.createdAt,
    this.projectName,
    this.contentType,
  });

  factory GeneratedContent.fromJson(Map<String, dynamic> j) =>
      GeneratedContent(
        id: j['id'],
        title: j['title'] ?? '',
        body: j['body'] ?? '',
        status: j['status'] ?? 1,
        overallScore: (j['overallScore'] ?? 0).toDouble(),
        imageUrl: j['imageUrl'],
        createdAt: DateTime.parse(j['createdAt']),
        projectName: j['projectName'],
        contentType: j['contentType'],
      );
}

class Project {
  final String id;
  final String agencyId;
  final String name;
  final String? description;
  final String? websiteUrl;
  final String? logoUrl;
  final bool isActive;
  final DateTime createdAt;
  final int contentSourcesCount;
  final int generatedContentsCount;
  final String? approvalMode;
  final int? autoApproveMinScore;

  Project({
    required this.id,
    required this.agencyId,
    required this.name,
    this.description,
    this.websiteUrl,
    this.logoUrl,
    required this.isActive,
    required this.createdAt,
    required this.contentSourcesCount,
    required this.generatedContentsCount,
    this.approvalMode,
    this.autoApproveMinScore,
  });

  factory Project.fromJson(Map<String, dynamic> j) => Project(
        id: j['id'],
        agencyId: j['agencyId'] ?? '',
        name: j['name'] ?? '',
        description: j['description'],
        websiteUrl: j['websiteUrl'],
        logoUrl: j['logoUrl'],
        isActive: j['isActive'] ?? true,
        createdAt: DateTime.parse(j['createdAt']),
        contentSourcesCount: j['contentSourcesCount'] ?? 0,
        generatedContentsCount: j['generatedContentsCount'] ?? 0,
        approvalMode: j['approvalMode']?.toString(),
        autoApproveMinScore: j['autoApproveMinScore'],
      );
}

class PendingApproval {
  final String id;
  final String agencyId;
  final String? projectId;
  final String? projectName;
  final int contentType;
  final String title;
  final String body;
  final int status;
  final double overallScore;
  final String? scoreExplanation;
  final String? imageUrl;
  final DateTime createdAt;

  PendingApproval({
    required this.id,
    required this.agencyId,
    this.projectId,
    this.projectName,
    required this.contentType,
    required this.title,
    required this.body,
    required this.status,
    required this.overallScore,
    this.scoreExplanation,
    this.imageUrl,
    required this.createdAt,
  });

  factory PendingApproval.fromJson(Map<String, dynamic> j) => PendingApproval(
        id: j['id'],
        agencyId: j['agencyId'] ?? '',
        projectId: j['projectId'],
        projectName: j['projectName'],
        contentType: j['contentType'] ?? 0,
        title: j['title'] ?? '',
        body: j['body'] ?? '',
        status: j['status'] ?? 2,
        overallScore: (j['overallScore'] ?? 0).toDouble(),
        scoreExplanation: j['scoreExplanation'],
        imageUrl: j['imageUrl'],
        createdAt: DateTime.parse(j['createdAt']),
      );
}

class AppNotification {
  final String id;
  final String agencyId;
  final String? jobId;
  final String? projectId;
  final String type;
  final String title;
  final String? body;
  final String? link;
  final bool read;
  final DateTime createdAt;
  final DateTime? readAt;

  AppNotification({
    required this.id,
    required this.agencyId,
    this.jobId,
    this.projectId,
    required this.type,
    required this.title,
    this.body,
    this.link,
    required this.read,
    required this.createdAt,
    this.readAt,
  });

  factory AppNotification.fromJson(Map<String, dynamic> j) => AppNotification(
        id: j['id'],
        agencyId: j['agencyId'] ?? '',
        jobId: j['jobId'],
        projectId: j['projectId'],
        type: j['type'] ?? '',
        title: j['title'] ?? '',
        body: j['body'],
        link: j['link'],
        read: j['read'] ?? false,
        createdAt: DateTime.parse(j['createdAt']),
        readAt: j['readAt'] != null ? DateTime.parse(j['readAt']) : null,
      );
}

class Job {
  final String id;
  final String agencyId;
  final String? projectId;
  final String agentType;
  final String status;
  final String? input;
  final String? output;
  final String? errorMessage;
  final DateTime createdAt;
  final DateTime? startedAt;
  final DateTime? completedAt;

  Job({
    required this.id,
    required this.agencyId,
    this.projectId,
    required this.agentType,
    required this.status,
    this.input,
    this.output,
    this.errorMessage,
    required this.createdAt,
    this.startedAt,
    this.completedAt,
  });

  factory Job.fromJson(Map<String, dynamic> j) => Job(
        id: j['id'],
        agencyId: j['agencyId'] ?? '',
        projectId: j['projectId'],
        agentType: j['agentType'] ?? '',
        status: j['status'] ?? '',
        input: j['input'],
        output: j['output'],
        errorMessage: j['errorMessage'],
        createdAt: DateTime.parse(j['createdAt']),
        startedAt:
            j['startedAt'] != null ? DateTime.parse(j['startedAt']) : null,
        completedAt:
            j['completedAt'] != null ? DateTime.parse(j['completedAt']) : null,
      );
}

class ContentSchedule {
  final String id;
  final String agencyId;
  final String? projectId;
  final String? projectName;
  final String name;
  final String timeOfDay;
  final String timeZone;
  final String? input;
  final bool isActive;
  final DateTime? lastRunAt;
  final DateTime? nextRunAt;
  final DateTime createdAt;

  ContentSchedule({
    required this.id,
    required this.agencyId,
    this.projectId,
    this.projectName,
    required this.name,
    required this.timeOfDay,
    required this.timeZone,
    this.input,
    required this.isActive,
    this.lastRunAt,
    this.nextRunAt,
    required this.createdAt,
  });

  factory ContentSchedule.fromJson(Map<String, dynamic> j) => ContentSchedule(
        id: j['id'],
        agencyId: j['agencyId'] ?? '',
        projectId: j['projectId'],
        projectName: j['projectName'],
        name: j['name'] ?? '',
        timeOfDay: j['timeOfDay'] ?? '',
        timeZone: j['timeZone'] ?? '',
        input: j['input'],
        isActive: j['isActive'] ?? false,
        lastRunAt:
            j['lastRunAt'] != null ? DateTime.parse(j['lastRunAt']) : null,
        nextRunAt:
            j['nextRunAt'] != null ? DateTime.parse(j['nextRunAt']) : null,
        createdAt: DateTime.parse(j['createdAt']),
      );
}
