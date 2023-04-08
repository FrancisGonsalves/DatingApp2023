import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';
import { Observable, map, take } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {

  constructor(private bsModalService: BsModalService) { }
  confirm(
    title = "Confirmation", 
    message = "Any unsaved changes will be lost, continue?",
    btnOkText = "Ok",
    btnCancelText = "Cancel")
    : Observable<boolean> 
  {
    const config = {
      initialState: {
        title, message, btnOkText, btnCancelText
      }
    };
    
    const bsModalRef: BsModalRef<ConfirmDialogComponent> = this.bsModalService.show(ConfirmDialogComponent, config);
    return bsModalRef.onHidden!.pipe(
      map(() => {
        return bsModalRef.content!.result;
      })
    );
  }
}
