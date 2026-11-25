document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll("form[onsubmit]").forEach(form => {
    form.addEventListener("submit", event => {
      if (form.getAttribute("onsubmit") === "return confirm('Delete this category?')" ||
          form.getAttribute("onsubmit") === "return confirm('Delete this food item?')") {
        if (!confirm("Are you sure you want to delete this record?")) event.preventDefault();
      }
    });
  });
});
