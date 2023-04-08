import { Component, OnInit } from '@angular/core';
import { Observable, take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { UserParams } from 'src/app/_models/userParams';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {

  members: Member[] = [];
  pagination: Pagination | undefined;
  genderList = [
    { value: "male", display: "Males" },
    { value: "female", display: "Females" }
  ];
  userParams: UserParams | undefined;
  constructor(private membersService: MembersService) { 
    this.userParams = this.membersService.getUserParams();
  }

  ngOnInit(): void {
    this.loadMembers();
  }
  loadMembers() {
    if(!this.userParams) 
      return;
    this.membersService.getMembers(this.userParams).subscribe({
      next: response => {
        if(response.result && response.pagination)
        {
          this.members = response.result;
          this.pagination = response.pagination;
        }
      }
    });
  }
  pageChanged($event: any)
  {
    if(this.userParams && this.userParams.pageNumber != $event.page)
    {
      this.userParams.pageNumber = $event.page;
      this.loadMembers();
    }
  }
  resetFilters() {
    this.userParams = this.membersService.resetUserParams();
    this.loadMembers();
  }

}
