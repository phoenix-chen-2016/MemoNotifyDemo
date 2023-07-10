import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AngularHttpClientAdapter } from './services/angular-http-client-adapter';
import { HttpClientModule } from '@angular/common/http';
import { MemoNotifyClient } from 'browser_client/services';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    HttpClientModule
  ],
  bootstrap: [AppComponent],
  providers: [
    { provide: AngularHttpClientAdapter, useClass: AngularHttpClientAdapter },
    { provide: MemoNotifyClient, useClass: MemoNotifyClient, deps: [AngularHttpClientAdapter] }
  ],
})
export class AppModule { }
