import { Component, OnInit } from "@angular/core";
import { Message } from "../_models/message";
import { MessageService } from "../_services/message.service";
import { Pagination } from "../_models/pagination";

@Component({
    selector: "app-messages",
    templateUrl: "./messages.component.html",
    styleUrls: ["./messages.component.css"]
})

export class MessagesComponent implements OnInit
{
    messages: Message[] = [];
    pagination?: Pagination;
    pageNumber: number = 1;
    pageSize: number = 5;
    container: string = "Unread";

    loading: boolean = false;

    constructor(private messageService: MessageService) {
    }

    ngOnInit() : void {
        this.loadMessages();
    }

    loadMessages() {
        this.loading = true;
        this.messageService.getMessages(this.pageNumber, this.pageSize, this.container).subscribe({
            next: response => {
                if(response.result && response.pagination) {
                    this.messages = response.result;
                    this.pagination = response.pagination;
                }
                this.loading = false;
            }
        });
    }

    pageChanged(event: any) {
        if(event.page != this.pageNumber) {
            this.pageNumber = event.page;
            this.loadMessages();
        }
    }
    deleteMessage(event: any, id: number) {
        this.messageService.deleteMessage(id).subscribe({
            next: _ => {
                this.messages?.splice(this.messages.findIndex(x => x.id == id), 1);
            }
        });
        event.stopPropagation();
    }
}