import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
    @ViewChild("messageForm") messageForm?: NgForm;
    @Input() userName?: string;
    messages: Message[] = [];
    messageContent: string = "";

    constructor(public messageService: MessageService) { }

    ngOnInit() { 
      //this.loadMessages();
    }

    loadMessages() {
      if(this.userName) { 
        this.messageService.getMessageThread(this.userName).subscribe({
          next: messages => this.messages = messages
        });
      }
    }

    sendMessage() {
      if(!this.userName) 
      return;
      this.messageService.sendMessage(this.userName, this.messageContent).then(
        () => this.messageForm?.reset()
      );
    }
}
