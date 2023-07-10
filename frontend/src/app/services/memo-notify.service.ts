import { Injectable } from '@angular/core';
import { MemoNotifyClient, MemoNotifyScheduleRequest } from 'browser_client/services';

@Injectable({
  providedIn: 'root',
})
export class MemoNotifyService {

  constructor(private _notifyClient: MemoNotifyClient) {

  }

  scheduleMemoNotify(memoRequest: MemoNotifyScheduleRequest): Promise<string | null> {
    return this._notifyClient.scheduleMemoNotify(memoRequest);
  }
}
