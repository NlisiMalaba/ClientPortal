import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

type QueryValue = string | number | boolean | null | undefined;
type QueryParams = object;

@Injectable({ providedIn: 'root' })
export class ApiClientService {
  constructor(private readonly httpClient: HttpClient) {}

  get<TResponse>(url: string, query?: QueryParams): Observable<TResponse> {
    return this.httpClient.get<TResponse>(url, { params: this.toHttpParams(query) });
  }

  getBlob(url: string, query?: QueryParams): Observable<Blob> {
    return this.httpClient.get(url, {
      params: this.toHttpParams(query),
      responseType: 'blob',
    });
  }

  post<TResponse, TBody = unknown>(
    url: string,
    body?: TBody,
    query?: QueryParams,
  ): Observable<TResponse> {
    return this.httpClient.post<TResponse>(url, body, {
      params: this.toHttpParams(query),
    });
  }

  put<TResponse, TBody = unknown>(
    url: string,
    body: TBody,
    query?: QueryParams,
  ): Observable<TResponse> {
    return this.httpClient.put<TResponse>(url, body, {
      params: this.toHttpParams(query),
    });
  }

  delete<TResponse>(url: string, query?: QueryParams): Observable<TResponse> {
    return this.httpClient.delete<TResponse>(url, {
      params: this.toHttpParams(query),
    });
  }

  private toHttpParams(query?: QueryParams): HttpParams | undefined {
    if (query === undefined) {
      return undefined;
    }

    let params = new HttpParams();
    for (const [key, value] of Object.entries(query as Record<string, QueryValue>)) {
      if (value === undefined || value === null) {
        continue;
      }

      params = params.set(key, String(value));
    }

    return params;
  }
}
