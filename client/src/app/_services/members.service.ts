import { Injectable } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "src/environments/environment";
import { Member } from "../_models/member";
import { getPaginationResult, getPaginationHeaders } from "./paginationHelper";
import { of, map, take } from "rxjs";
import { UserParams } from "../_models/userParams";
import { AccountService } from "./account.service";
import { User } from "../_models/user";

@Injectable({
    providedIn: "root"
})

export class MembersService
{
    baseUrl: string = environment.baseUrl;
    members: Member[] = [];
    memberCache = new Map();
    userParams: UserParams | undefined;
    user: User | undefined;

    constructor(private http: HttpClient, private accountService: AccountService) {
        this.accountService.currentUser$.pipe(take(1)).subscribe({
            next: user => {
                if(user) {
                    this.user = user;
                    this.resetUserParams();
                }
            }
        });
    }

    getUserParams() {
        return this.userParams;
    }
    setUserParams(userParams: UserParams)
    {
        this.userParams = userParams;
    }
    resetUserParams() {
        if(this.user) {
            this.userParams = new UserParams(this.user);
            return this.userParams;
        }
        return;
    }

    getMembers(userParams: UserParams) {

        const response = this.memberCache.get(Object.values(userParams).join('-'));
        if(response)
            return of(response);

        let params = getPaginationHeaders(userParams.pageNumber, userParams.pageSize);

        params = params.append("gender", userParams.gender);
        params = params.append("minAge", userParams.minAge);
        params = params.append("maxAge", userParams.maxAge);
        params = params.append("orderBy", userParams.orderBy);

        return getPaginationResult<Member[]>(this.baseUrl + "users", params, this.http).pipe(
            map(response => {
                if(response.pagination && response.result)
                    this.memberCache.set(Object.values(userParams).join('-'), response);
                return response;
            })
        );
    }
    getMember(userName: string)
    {
        const member = [...this.memberCache.values()]
            .reduce((arr, ele) => arr.concat(ele.result), [])
            .find((x: Member) => x.userName === userName);
        if(member)
            return of(member);
        return this.http.get<Member>(this.baseUrl + "users/" + userName);
    }
    updateMember(member: Member) {
        return this.http.put(this.baseUrl + "users", member).pipe(
            map(() => {
                const index = this.members.indexOf(member);
                this.members[index] = {...this.members[index], ...member};
            })
        );
    }
    setMainPhoto(photoId: number) {
        return this.http.put(this.baseUrl + "users/set-main-photo/" + photoId, {});
    }
    deletePhoto(photoId: number) {
        return this.http.delete(this.baseUrl + "users/delete-photo/" + photoId, {});
    }
    addLike(userName: string) {
        return this.http.post(this.baseUrl + "likes/" + userName, {});
    }
    getLikes(predicate: string, pageNumber: number, pageSize: number) {
        var params = getPaginationHeaders(pageNumber, pageSize);
        params = params.append("predicate", predicate);

        return getPaginationResult<Member[]>(this.baseUrl + "likes", params, this.http);
    }
    getHttpOptions() {
        const userString = localStorage.getItem("user");
        if(!userString) return;
        const user = JSON.parse(userString);
        return {
            headers: {
                authorization: "Bearer " + user.token
            }
        }
    }
}