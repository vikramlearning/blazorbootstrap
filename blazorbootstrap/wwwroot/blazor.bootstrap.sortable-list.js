
export function initialize(dotNetHelper, elementId) {
    let listGroupEl = document.getElementById(elementId);
    if (listGroupEl == null)
        return;
    const sortable = Sortable.create(listGroupEl);
}
