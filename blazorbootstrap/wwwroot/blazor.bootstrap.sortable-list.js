export function initialize(dotNetHelper, elementId, handle, group) {
    let listGroupEl = document.getElementById(elementId);
    if (listGroupEl == null)
        return;

    if (Sortable) {
        const sortable = Sortable.create(listGroupEl, {
            animation: 150,
            group: group,
            handle: handle, // handle's class
        });
    }
}
