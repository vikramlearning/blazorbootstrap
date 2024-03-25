
export function initialize(dotNetHelper, elementId, handle) {
    let listGroupEl = document.getElementById(elementId);
    if (listGroupEl == null)
        return;

    if (Sortable) {
        const sortable = Sortable.create(listGroupEl, {
            handle: handle, // handle's class
            animation: 150            
        });
    }
}
