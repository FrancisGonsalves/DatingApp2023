import { Injectable } from "@angular/core";
import { environment } from "src/environments/environment";
import { HttpClient } from "@angular/common/http";
import { getPaginationHeaders, getPaginationResult } from "./paginationHelper";
import { Message } from "../_models/message";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { BehaviorSubject, take } from "rxjs";
import { User } from "../_models/user";
import { Group } from "../_models/group";

@Injectable({
    providedIn: "root"
})

export class MessageService
{
    baseUrl: string = environment.baseUrl;
    hubUrl: string = environment.hubUrl;
    private hubConnection?: HubConnection;
    private messageThreadSource = new BehaviorSubject<Message[]>([]);
    messageThread$ = this.messageThreadSource.asObservable();

    constructor(private http: HttpClient) { }

    createHubConnection(user: User, otherUserName: string)
    {
        this.stopHubConnection();
        this.hubConnection = new HubConnectionBuilder()
        .withUrl(this.hubUrl + "message?user=" + otherUserName, {
            accessTokenFactory: () => user.token
        })
        .withAutomaticReconnect()
        .build();

        this.hubConnection.start().catch(error => console.log(error));

        this.hubConnection.on("ReceiveMessageThread", messages => {
            this.messageThreadSource.next(messages);
        });

        this.hubConnection.on("NewMessage", message => {
            this.messageThread$.pipe(take(1)).subscribe({
                next: messages => {
                    this.messageThreadSource.next([...messages, message]);
                }
            });
        })

        this.hubConnection.on("UpdatedGroup", (group: Group) => {
            if(group.connections.some(c => c.userName == otherUserName))
            {
                this.messageThread$.pipe(take(1)).subscribe({
                    next: messages => {
                        messages.forEach(message => {
                            if(!message.dateRead)
                                message.dateRead = new Date(Date.now());
                        });
                        this.messageThreadSource.next([...messages]);
                    }
                });
            }
        })
    }

    stopHubConnection() {
        if(this.hubConnection)
            this.hubConnection.stop();
    }

    getMessages(pageNumber: number, pageSize: number, container: string) {
        var params = getPaginationHeaders(pageNumber, pageSize);
        params = params.append("container", container);
        
        return getPaginationResult<Message[]>(this.baseUrl + "messages", params, this.http);
    }
    getMessageThread(userName: string) {
        return this.http.get<Message[]>(this.baseUrl + "messages/thread/" + userName);
    }
    async sendMessage(userName: string, content: string)
    {
        return this.hubConnection?.invoke("SendMessage", { recipientUserName: userName, content });
    }
    deleteMessage(id: number) {
        return this.http.delete(this.baseUrl + "messages/" + id);
    }
}