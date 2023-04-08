import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "src/environments/environment";
import { User } from "../_models/user";

@Injectable({
    providedIn: "root"
})

export class AdminService
{
    baseUrl: string = environment.baseUrl;
    constructor(private http: HttpClient) { }

    getUsersWithRoles() {
        return this.http.get<User[]>(this.baseUrl + "admin/users-with-roles");
    }
    editRoles(userName: string, roles: string[]) {
        return this.http.post<string[]>(this.baseUrl + "admin/edit-roles/" + userName + "?roles=" + roles, {});
    }
}