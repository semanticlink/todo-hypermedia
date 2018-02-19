import { uriMapping } from './util/UriMapping';
import { log } from 'semanticLink';

export const apiUri = document.querySelector('HEAD link[rel="api"]').href;
export const authenticatorUri = document.querySelector('HEAD link[rel="authenticator"]').href;
export const clientUri = window.location.href;

log.info(`Client uri: '${clientUri}'`);
log.info(`Api uri: '${apiUri}'`);

/**
 * KLUDGE: the apiUri and the root api ARE NOT THE SAME in the current implementation
 *
 * This hack removes the initial api 'home/' so that we can do correct rewriting. Once 'home' is removed from
 * the api we can remove this hack.
 */
uriMapping(clientUri, apiUri.replace('home/', ''));

