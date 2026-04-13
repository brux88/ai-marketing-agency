export interface ApiResponse<T> {
  data: T;
  success: boolean;
  error?: string;
}

export interface UserInfo {
  id: string;
  email: string;
  fullName: string;
  tenantId: string;
  role: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfo;
}

export interface Agency {
  id: string;
  name: string;
  productName: string;
  description?: string;
  websiteUrl?: string;
  logoUrl?: string;
  brandVoice: BrandVoice;
  targetAudience: TargetAudience;
  defaultLlmProviderKeyId?: string | null;
  imageLlmProviderKeyId?: string | null;
  approvalMode: ApprovalMode;
  autoApproveMinScore: number;
  isActive: boolean;
  createdAt: string;
  contentSourcesCount: number;
  generatedContentsCount: number;
  enableLogoOverlay?: boolean;
  logoOverlayPosition?: number;
}

export enum LogoPosition {
  TopLeft = 0,
  TopRight = 1,
  BottomLeft = 2,
  BottomRight = 3,
}

export enum ImageGenerationMode {
  None = 0,
  Single = 1,
  Carousel = 2,
}

export interface BrandVoice {
  tone: string;
  style: string;
  keywords: string[];
  examplePhrases: string[];
  forbiddenWords: string[];
  language: string;
}

export interface TargetAudience {
  description: string;
  ageRange?: string;
  interests: string[];
  painPoints: string[];
  personas: PersonaProfile[];
}

export interface PersonaProfile {
  name: string;
  jobTitle?: string;
  description?: string;
}

export interface LlmKey {
  id: string;
  providerType: number;
  displayName: string;
  maskedKey: string;
  modelName?: string;
  baseUrl?: string;
  category?: number;
  hasApiKeySecret?: boolean;
  isActive: boolean;
  createdAt: string;
}

export enum LlmProviderCategory {
  Text = 0,
  Image = 1,
}

export interface GeneratedContent {
  id: string;
  agencyId: string;
  contentType: number;
  title: string;
  body: string;
  status: ContentStatus;
  qualityScore: number;
  relevanceScore: number;
  seoScore: number;
  brandVoiceScore: number;
  overallScore: number;
  scoreExplanation?: string;
  autoApproved: boolean;
  imageUrl?: string | null;
  imagePrompt?: string | null;
  imageUrls?: string[] | null;
  videoUrl?: string | null;
  projectId?: string | null;
  createdAt: string;
  approvedAt?: string;
}

export enum ApprovalMode {
  Manual = 1,
  AutoApprove = 2,
  AutoApproveAboveScore = 3,
}

export enum ContentStatus {
  Draft = 1,
  InReview = 2,
  Approved = 3,
  Published = 4,
  Rejected = 5,
}

export enum LlmProviderType {
  OpenAI = 1,
  Anthropic = 2,
  NanoBanana = 3,
  AzureOpenAI = 4,
  Custom = 5,
}

export enum ContentSourceType {
  RssFeed = 1,
  Website = 2,
  SocialAccount = 3,
}

export interface ContentSource {
  id: string;
  agencyId: string;
  projectId?: string | null;
  type: ContentSourceType;
  url: string;
  name?: string | null;
  config?: string | null;
  isActive: boolean;
  lastFetchedAt?: string | null;
  createdAt: string;
}

export enum AgentType {
  ContentWriter = 1,
  SocialManager = 2,
  Newsletter = 3,
  Analytics = 4,
  ContentStrategist = 5,
  SeoOptimizer = 6,
}

export enum DayOfWeekFlag {
  None = 0,
  Monday = 1,
  Tuesday = 2,
  Wednesday = 4,
  Thursday = 8,
  Friday = 16,
  Saturday = 32,
  Sunday = 64,
  Weekdays = 31,
  Weekend = 96,
  EveryDay = 127,
}

export interface ContentSchedule {
  id: string;
  agencyId: string;
  projectId?: string | null;
  projectName?: string | null;
  name: string;
  days: number;
  timeOfDay: string;
  timeZone: string;
  agentType: number;
  input?: string | null;
  isActive: boolean;
  lastRunAt?: string | null;
  nextRunAt?: string | null;
  createdAt: string;
}

export interface PendingApproval {
  id: string;
  agencyId: string;
  projectId?: string | null;
  projectName?: string | null;
  contentType: number;
  title: string;
  body: string;
  status: ContentStatus;
  overallScore: number;
  scoreExplanation?: string | null;
  imageUrl?: string | null;
  createdAt: string;
}

export interface Project {
  id: string;
  agencyId: string;
  name: string;
  description?: string | null;
  websiteUrl?: string | null;
  logoUrl?: string | null;
  brandVoice: BrandVoice;
  targetAudience: TargetAudience;
  isActive: boolean;
  createdAt: string;
  contentSourcesCount: number;
  generatedContentsCount: number;
}

export enum SocialPlatform {
  Twitter = 1,
  LinkedIn = 2,
  Instagram = 3,
  Facebook = 4,
}

export interface SocialConnector {
  id: string;
  platform: SocialPlatform;
  platformName: string;
  accountId?: string | null;
  accountName?: string | null;
  profileImageUrl?: string | null;
  isActive: boolean;
  tokenExpiresAt?: string | null;
  createdAt: string;
}

export interface PublishResult {
  success: boolean;
  postId?: string | null;
  postUrl?: string | null;
  error?: string | null;
}

export enum EmailProviderType {
  Smtp = 1,
  SendGrid = 2,
}

export interface EmailConnectorDto {
  id: string;
  providerType: EmailProviderType;
  smtpHost?: string | null;
  smtpPort?: number | null;
  smtpUsername?: string | null;
  hasSmtpPassword: boolean;
  hasApiKey: boolean;
  fromEmail: string;
  fromName: string;
  isActive: boolean;
}

export interface NewsletterSubscriber {
  id: string;
  email: string;
  name?: string | null;
  isActive: boolean;
  subscribedAt: string;
}

export interface EmailSendResult {
  success: boolean;
  sentCount: number;
  failedCount: number;
  error?: string | null;
}
