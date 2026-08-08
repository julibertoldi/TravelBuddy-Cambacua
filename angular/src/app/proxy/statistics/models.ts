
export interface AdminDashboardDto {
  totalSearches: number;
  totalSavedDestinations: number;
  topDestinations: DestinationStatDto[];
  totalApiCalls: number;
  averageResponseTimeMs: number;
  totalApiErrors: number;
}

export interface ApiCallLogDto {
  id?: string;
  endpoint?: string;
  statusCode: number;
  responseTimeMs: number;
  isSuccess: boolean;
  errorMessage?: string;
  timestamp?: string;
}

export interface DestinationStatDto {
  destinationName?: string;
  viewCount: number;
}
