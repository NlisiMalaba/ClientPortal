export interface PagedResult<TItem> {
  items: TItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface ApiOperationResult {
  success: boolean;
  message?: string;
}

export interface EntitySummary {
  id: string;
  [key: string]: unknown;
}
