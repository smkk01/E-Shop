import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatToolbarModule } from "@angular/material/toolbar";
import { RouterModule } from "@angular/router";
import { UsersService } from './services/users.service';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';

import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import {
  MsalService,
  MsalBroadcastService,
  MSAL_GUARD_CONFIG,
  MsalGuardConfiguration,
} from '@azure/msal-angular';
import { LoginService } from './services/login.service';
import {
  InteractionStatus,
  RedirectRequest,
  EventMessage,
  EventType,
} from '@azure/msal-browser';
import { environment } from '../environment';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, MatButtonModule, MatIconModule, MatInputModule, MatFormFieldModule, MatToolbarModule, RouterModule, RouterLinkActive, ReactiveFormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  searchForm: FormGroup;

  constructor(public usersService: UsersService, private router: Router, private fb: FormBuilder,
    @Inject(MSAL_GUARD_CONFIG) private msalGuardConfig: MsalGuardConfiguration,
    private authService: MsalService,
    private msalBroadcastService: MsalBroadcastService,
    private loginService: LoginService,
  ) {
    this.searchForm = this.fb.group({
      searchStr: ['', []]
    });
  }

  search() {
    this.router.navigateByUrl(`/products/search/${this.searchForm.value.searchStr}`);
  }

  /////////////

  isIframe = false;
  private readonly _destroying$ = new Subject<void>();


  ngOnInit(): void {
    this.authService.handleRedirectObservable().subscribe();
    this.isIframe = window !== window.parent && !window.opener; // Remove this line to use Angular Universal

    this.usersService.setIsAuthenticated(this.authService);

    this.authService.instance.enableAccountStorageEvents(); // Optional - This will enable ACCOUNT_ADDED and ACCOUNT_REMOVED events emitted when a user logs in or out of another tab or window
    this.msalBroadcastService.msalSubject$
      .pipe(
        filter(
          (msg: EventMessage) =>
            msg.eventType === EventType.ACCOUNT_ADDED ||
            msg.eventType === EventType.ACCOUNT_REMOVED
        )
      )
      .subscribe((result: EventMessage) => {
        if (this.authService.instance.getAllAccounts().length === 0) {
          window.location.pathname = '/';
        } else {
          this.usersService.setIsAuthenticated(this.authService);
        }
      });

    //To subscribe for claims
    this.loginService.claims$.subscribe((c) => {
      this.usersService.setClaims(c);
    });

    this.msalBroadcastService.inProgress$
      .pipe(
        filter(
          (status: InteractionStatus) => status === InteractionStatus.None
        ),
        takeUntil(this._destroying$)
      )
      .subscribe(() => {
        this.usersService.setIsAuthenticated(this.authService);
        this.checkAndSetActiveAccount();
      });
  }


  checkAndSetActiveAccount() {
    let activeAccount = this.authService.instance.getActiveAccount();

    if (
      !activeAccount &&
      this.authService.instance.getAllAccounts().length > 0
    ) {
      let accounts = this.authService.instance.getAllAccounts();
      this.authService.instance.setActiveAccount(accounts[0]);
    }
  }

  loginRedirect() {
    if (this.msalGuardConfig.authRequest) {
      this.authService.loginRedirect({
        ...this.msalGuardConfig.authRequest,
      } as RedirectRequest);
    } else {
      this.authService.loginRedirect();
    }
  }

  getCurrentDomain(): string {
    const { protocol, hostname, port } = window.location;
    return `${protocol}//${hostname}${port ? ':' + port : ''}`;
  }

  signUpRedirect() {
    // Use environment config to construct the authority URL dynamically
    const authority = `https://${environment.config.adb2cConfig.b2clogin}/${environment.config.adb2cConfig.authorityDomain}/${environment.config.adb2cConfig.signUpUserFlow}`;

    // Create the request object based on your auth request settings
    const request: RedirectRequest = {
      scopes: ['openid'], // Adjust scopes as needed
      redirectUri: this.getCurrentDomain(),
      authority: authority, // Use the constructed authority
    };

    // Redirect to the sign-in flow
    this.authService.loginRedirect(request);
  }

  logout() {
    this.authService.logoutRedirect();
  }

  ngOnDestroy(): void {
    this._destroying$.next(undefined);
    this._destroying$.complete();
  }
}
