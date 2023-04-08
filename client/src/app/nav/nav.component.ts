import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
    model: any = {};
    currentUser$: Observable<User | null> = of(null);

    constructor(private accountService: AccountService, private router: Router, private toastr: ToastrService) { }

    ngOnInit() {
      this.currentUser$ = this.accountService.currentUser$;
    }

    login(): void {
      this.accountService.login(this.model).subscribe({
        next: _ => this.router.navigateByUrl("/members"),
        error: error => console.log(error)
      });
    }

    logout(): void {
      this.router.navigateByUrl("/")
      this.accountService.logout();
    }
}
