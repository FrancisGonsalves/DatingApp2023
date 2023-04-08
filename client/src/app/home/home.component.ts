import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
    registerMode: boolean = false;
    users: any;

    constructor(private http: HttpClient) { }

    registerToggle(): void {
      this.registerMode = !this.registerMode;
    }

    cancelRegisterMode(event: boolean) {
      this.registerMode = event;
    }
}
