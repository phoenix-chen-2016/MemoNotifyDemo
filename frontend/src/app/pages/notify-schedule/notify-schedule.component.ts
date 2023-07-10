import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { Component } from '@angular/core';
import { MatFormFieldModule, MAT_FORM_FIELD_DEFAULT_OPTIONS } from '@angular/material/form-field';
import { HttpClientModule } from '@angular/common/http';
import { MatInputModule } from '@angular/material/input';
import {
  NgxMatDatetimePickerModule,
  NgxMatTimepickerModule,
  NGX_MAT_DATE_FORMATS
} from '@angular-material-components/datetime-picker';
import * as moment from 'moment';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { MemoNotifyService } from 'src/app/services/memo-notify.service';
import { MemoNotifyScheduleRequest } from 'browser_client/services';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgxMatMomentModule } from '@angular-material-components/moment-adapter';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-notify-schedule',
  templateUrl: './notify-schedule.component.html',
  styleUrls: ['./notify-schedule.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    HttpClientModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatCardModule,
    MatDatepickerModule,
    NgxMatDatetimePickerModule,
    NgxMatTimepickerModule,
    ReactiveFormsModule,
    MatChipsModule,
    MatSnackBarModule,
    NgxMatMomentModule
  ],
  providers: [
    {
      provide: MAT_FORM_FIELD_DEFAULT_OPTIONS,
      useValue: { appearance: 'outline' }
    },
    {
      provide: NGX_MAT_DATE_FORMATS,
      useValue: {
        parse: {
          dateInput: "YYYY-MM-DD LT"
        },
        display: {
          dateInput: "YYYY-MM-DD LT",
          monthYearLabel: "YYYY-MM",
          dateA11yLabel: "LT",
          monthYearA11yLabel: "YYYY MM"
        }
      }
    }
  ]
})
export class NotifyScheduleComponent {
  readonly separatorKeysCodes = [ENTER, COMMA] as const;
  scheduleForm: FormGroup;
  currentDate = moment(new Date().setSeconds(0));
  gpCodes: Set<string> = new Set<string>();

  constructor(
    private _fb: FormBuilder,
    private _notifyService: MemoNotifyService,
    private _snackBar: MatSnackBar
  ) {
    this.scheduleForm = this._fb.group({
      scheduleTime: this.currentDate,
      title: "",
      description: ""
    });
  }

  addGameProvider(event: MatChipInputEvent) {
    const value = (event.value || '').trim();

    if (value)
      this.gpCodes.add(value);

    event.chipInput.clear();
  }

  removeGameProvider(brandCode: string) {
    this.gpCodes.delete(brandCode);
  }

  async submit() {
    try {
      let scheduleRequest: MemoNotifyScheduleRequest = {
        ...this.scheduleForm.value,
        gpCodes: Array.from(this.gpCodes)
      };

      await this._notifyService.scheduleMemoNotify(scheduleRequest);

      this._snackBar.open(
        "Notify memoed!",
        undefined,
        {
          duration: 3000,
          verticalPosition: "top"
        });
    }
    catch (error) {
      console.error(error);
      this._snackBar.open(
        `Notify memo fail!`,
        "Error",
        {
          duration: 3000,
          verticalPosition: "top"
        });
    }
  }
}
