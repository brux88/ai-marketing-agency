import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class ApiClient {
  static const bool _useLocal = false;
  static const String baseUrl =
      _useLocal ? 'http://10.0.2.2:5130' : 'https://wepostai-api.azurewebsites.net';
  static const _storage = FlutterSecureStorage();

  static Future<String?> get token => _storage.read(key: 'access_token');
  static Future<String?> get refreshToken => _storage.read(key: 'refresh_token');

  static Future<void> setTokens(String accessToken, String? refreshToken) async {
    await _storage.write(key: 'access_token', value: accessToken);
    if (refreshToken != null) {
      await _storage.write(key: 'refresh_token', value: refreshToken);
    }
  }

  static Future<void> setToken(String token) =>
      _storage.write(key: 'access_token', value: token);

  static Future<void> clearToken() async {
    await _storage.delete(key: 'access_token');
    await _storage.delete(key: 'refresh_token');
  }

  static Future<Map<String, String>> _headers() async {
    final t = await token;
    return {
      'Content-Type': 'application/json',
      if (t != null) 'Authorization': 'Bearer $t',
    };
  }

  static bool _refreshing = false;

  static Future<bool> _tryRefresh() async {
    if (_refreshing) return false;
    _refreshing = true;
    try {
      final rt = await refreshToken;
      if (rt == null || rt.isEmpty) return false;
      final res = await http.post(
        Uri.parse('$baseUrl/api/v1/auth/refresh'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'refreshToken': rt}),
      );
      if (res.statusCode >= 200 && res.statusCode < 300) {
        final body = jsonDecode(res.body);
        final data = body['data'];
        if (data != null) {
          await setTokens(
            data['accessToken'] as String,
            data['refreshToken'] as String?,
          );
          return true;
        }
      }
      await clearToken();
      return false;
    } catch (_) {
      return false;
    } finally {
      _refreshing = false;
    }
  }

  static Future<dynamic> _send(
    Future<http.Response> Function(Map<String, String> headers) sender,
  ) async {
    var res = await sender(await _headers());
    if (res.statusCode == 401 && await _tryRefresh()) {
      res = await sender(await _headers());
    }
    return _handle(res);
  }

  static Future<dynamic> get(String path) =>
      _send((h) => http.get(Uri.parse('$baseUrl$path'), headers: h));

  static Future<dynamic> post(String path, [Map<String, dynamic>? body]) =>
      _send((h) => http.post(
            Uri.parse('$baseUrl$path'),
            headers: h,
            body: body == null ? null : jsonEncode(body),
          ));

  static Future<dynamic> put(String path, Map<String, dynamic> body) =>
      _send((h) => http.put(
            Uri.parse('$baseUrl$path'),
            headers: h,
            body: jsonEncode(body),
          ));

  static Future<dynamic> delete(String path) =>
      _send((h) => http.delete(Uri.parse('$baseUrl$path'), headers: h));

  static dynamic _handle(http.Response res) {
    if (res.statusCode >= 200 && res.statusCode < 300) {
      if (res.body.isEmpty) return null;
      return jsonDecode(res.body);
    }
    throw ApiException(res.statusCode, res.body);
  }
}

class ApiException implements Exception {
  final int statusCode;
  final String body;
  ApiException(this.statusCode, this.body);

  @override
  String toString() => 'HTTP $statusCode: $body';
}
