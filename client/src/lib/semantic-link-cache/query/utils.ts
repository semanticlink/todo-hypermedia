import {Representation} from "./interfaces";
import {FormItem, FormRepresentation} from "../interfaces";

/**
 * A guard to detect whether the object is a collection {@link Representation}
 *
 * @see https://stackoverflow.com/questions/14425568/interface-type-check-with-typescript
 * @param object
 * @returns whether the object is an instance on the interface
 */
export function instanceOfCollection(object: any): object is Representation {
    // note this check may not be strict enough as we progress. It may need to check
    // that the items of type type FeedItem
    return 'items' in object;
}

/**
 * A guard to detect whether the object is a form {@link FormRepresentation}
 *
 * @see https://stackoverflow.com/questions/14425568/interface-type-check-with-typescript
 * @param object
 * @returns whether the object is an instance on the interface
 */
export function instanceOfForm(object: any): object is FormRepresentation {
    if ('items' in object) {
        const [first,]: FormItem[] = object['items'];
        return first != undefined && 'type' in first;
    }
    return false;
}
