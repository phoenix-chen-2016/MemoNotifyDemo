import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from "@angular/common/http";
import { HttpClientAdapter, HttpClientResponse } from "browser_client/services";
import { firstValueFrom, tap } from 'rxjs';

@Injectable()
export class AngularHttpClientAdapter implements HttpClientAdapter {

  constructor(private _http: HttpClient) {

  }

  put<TRequest, TBody>(url: string, request: TRequest): Promise<HttpClientResponse<TBody | null>> {
    return firstValueFrom(this._http.put<HttpResponse<TBody>>(url, request).pipe(tap(res => console.log(res))))
      .then(res => {
        return {
          statusCode: res.status,
          body: res.body
        };
      });
  }

}
