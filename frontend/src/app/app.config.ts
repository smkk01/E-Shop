import {
  ApplicationConfig,
  importProvidersFrom,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideRouter, withHashLocation } from '@angular/router';
import { routes } from './app.routes';
import { BrowserModule } from '@angular/platform-browser';
import {
  provideHttpClient,
  withInterceptorsFromDi,
  HTTP_INTERCEPTORS,
  withFetch,
} from '@angular/common/http';
import {
  provideAnimations,
  provideNoopAnimations,
} from '@angular/platform-browser/animations';
import {
  IPublicClientApplication,
  PublicClientApplication,
  InteractionType,
  BrowserCacheLocation,
  LogLevel,
} from '@azure/msal-browser';
import {
  MsalInterceptor,
  MSAL_INSTANCE,
  MsalInterceptorConfiguration,
  MsalGuardConfiguration,
  MSAL_GUARD_CONFIG,
  MSAL_INTERCEPTOR_CONFIG,
  MsalService,
  MsalGuard,
  MsalBroadcastService,
} from '@azure/msal-angular';
import { environment, b2c } from '../environment';

export function loggerCallback(logLevel: LogLevel, message: string) {
  console.log(message);
}

const scopeNames = {
  signUpSignIn: 'B2C_1_SignUp_SignIn',
  resetPassword: 'B2C_1_ResetPassword',
  editProfile: 'B2C_1_EditProfile',
}
export const b2cPolicies = {
  names: scopeNames,
  authorities: {
    signUpSignIn: {
      authority:
        `https://${b2c.tenantName}.b2clogin.com/${b2c.tenantName}.onmicrosoft.com/${scopeNames.signUpSignIn}`,
    },
    resetPassword: {
      authority:
        `https://${b2c.tenantName}.b2clogin.com/${b2c.tenantName}.onmicrosoft.com/${scopeNames.resetPassword}`,
    },
    editProfile: {
      authority:
        `https://${b2c.tenantName}.b2clogin.com/${b2c.tenantName}.onmicrosoft.com/${scopeNames.editProfile}`,
    },
  },
  authorityDomain: `${b2c.tenantName}.b2clogin.com`,
};

export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication({
    auth: {
      clientId: environment.config.adb2cConfig.clientId,
      authority: b2cPolicies.authorities.signUpSignIn.authority, //environment.msalConfig.auth.authority,
      knownAuthorities: [b2cPolicies.authorityDomain], // Mark your B2C tenant's domain as trusted.
      redirectUri: '/',
      postLogoutRedirectUri: '/',
    },
    cache: {
      cacheLocation: BrowserCacheLocation.LocalStorage,
    },
    system: {
      allowNativeBroker: false, // Disables WAM Broker
      loggerOptions: {
        loggerCallback,
        logLevel: LogLevel.Info,
        piiLoggingEnabled: false,
      },
    },
  });
}

export function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {
  const protectedResourceMap = new Map<string, Array<string>>();
  //have this set if more microservice used or requires different scope for different controllers
  environment.config.adb2cConfig.apiEndpointUrls.forEach(apiEndpointUrl => {
    protectedResourceMap.set(
      apiEndpointUrl, // This is for all controllers
      environment.config.adb2cConfig.scopeUrls
    );

  });
  
  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap,
  };
}

export function MSALGuardConfigFactory(): MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Redirect,
    authRequest: {
      scopes: [...environment.config.adb2cConfig.scopeUrls],
    },
    loginFailedRoute: '/',
  };
}
//provideRouter(routes,withRouterConfig({ onSameUrlNavigation: 'reload' })),
export const appConfig: ApplicationConfig = {
  providers: [    
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withHashLocation()),
    provideAnimations(), provideHttpClient(),
    importProvidersFrom(
      BrowserModule      
    ),
    provideNoopAnimations(),
    provideHttpClient(withInterceptorsFromDi(), withFetch()),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true,
    },
    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory,
    },
    {
      provide: MSAL_GUARD_CONFIG,
      useFactory: MSALGuardConfigFactory,
    },
    {
      provide: MSAL_INTERCEPTOR_CONFIG,
      useFactory: MSALInterceptorConfigFactory,
    },
    MsalService,
    MsalGuard,
    MsalBroadcastService,
  ],
};
