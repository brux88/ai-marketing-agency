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

  GeneratedContent({
    required this.id,
    required this.title,
    required this.body,
    required this.status,
    required this.overallScore,
    this.imageUrl,
    required this.createdAt,
  });

  factory GeneratedContent.fromJson(Map<String, dynamic> j) => GeneratedContent(
        id: j['id'],
        title: j['title'] ?? '',
        body: j['body'] ?? '',
        status: j['status'] ?? 1,
        overallScore: (j['overallScore'] ?? 0).toDouble(),
        imageUrl: j['imageUrl'],
        createdAt: DateTime.parse(j['createdAt']),
      );
}
