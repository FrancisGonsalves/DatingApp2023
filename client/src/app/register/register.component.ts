import { Component, OnInit, Output, EventEmitter } from "@angular/core";
import { AccountService } from "../_services/account.service";
import { ToastrService } from "ngx-toastr";
import { FormGroup, FormControl, Validators, ValidatorFn, AbstractControl, FormBuilder } from "@angular/forms";
import { Router } from "@angular/router";

@Component({
  selector: "app-register",
  templateUrl: "./register.component.html",
  styleUrls: ["./register.component.css"]
})

export class RegisterComponent implements OnInit
{
  @Output() cancelRegister = new EventEmitter();
  maxDate: Date = new Date();
  validationErrors: string[] | undefined;

  registerForm: FormGroup = new FormGroup({});

  constructor(private accountService: AccountService, private toastr: ToastrService, private fb: FormBuilder, private router: Router) { }

  ngOnInit() {
    this.initializeForm();
  }

  initializeForm() {
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
    this.registerForm = this.fb.group({
      gender: ["male"],
      userName: ["", Validators.required],
      knownAs: ["", Validators.required],
      dateOfBirth: ["", Validators.required],
      city: ["", Validators.required],
      country: ["", Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchValues("password")]]
    });

    this.registerForm.controls["password"].valueChanges.subscribe({
      next: () => this.registerForm.controls["confirmPassword"].updateValueAndValidity()
    })
  }

  matchValues(matchTo: string): ValidatorFn
  {
    return (control: AbstractControl) => {
      return control.value == control.parent?.get(matchTo)?.value ? null : { notMatching: true }
    }
  }

  register() {
    const values = { ...this.registerForm.value, dateOfBirth: this.getDateOnly(this.registerForm.controls["dateOfBirth"].value) }
    
    if(this.registerForm.valid)
    {

    this.accountService.register(values).subscribe({
      next: response => {
        this.router.navigateByUrl("/members");
      },
      error: error => {
        this.validationErrors = error;
      }
    });

    }
    // this.accountService.register(this.model).subscribe({
    //   next: response => {
    //     console.log(response);
    //     this.cancel();
    //   },
    //   error: error => console.log(error)
    // });
  }
  cancel() {
    this.cancelRegister.emit(false);
  }

  getDateOnly(dob: string) {
    let date = new Date(dob);
    return new Date(date.setMinutes(date.getMinutes() - date.getTimezoneOffset())).toISOString().slice(0, 10);
  }
}