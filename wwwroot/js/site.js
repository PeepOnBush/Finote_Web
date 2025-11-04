document.addEventListener("DOMContentLoaded", function() {

    // Logic for the sidebar dropdown menu
    const dropdownToggles = document.querySelectorAll('.sidebar-dropdown-toggle');

    dropdownToggles.forEach(toggle => {
        toggle.addEventListener('click', function(event) {
            event.preventDefault(); // Prevent page from navigating away
            
            const dropdownMenu = this.nextElementSibling;
            
            // Toggle the 'open' class on the link for the arrow icon
            this.classList.toggle('open');
            
            // Toggle the display of the menu
            if (dropdownMenu.style.maxHeight) {
                // If menu is open, close it
                dropdownMenu.style.maxHeight = null;
            } else {
                // If menu is closed, open it by setting max-height to its scroll height
                dropdownMenu.style.maxHeight = dropdownMenu.scrollHeight + "px";
            }
        });
    });

});
const deleteButtons = document.querySelectorAll('.delete-btn');

deleteButtons.forEach(button => {
    button.addEventListener('click', function(event) {
        // Prevent the link from navigating immediately
        event.preventDefault();

        // Show a confirmation popup
        const confirmed = confirm('Are you sure you want to delete this user? This action cannot be undone.');

        if (confirmed) {
            // In a real application, you would make an API call here.
            // For now, we'll just log to the console and remove the row from the view.
            console.log('User deletion confirmed.');
            this.closest('tr').remove(); // Removes the table row
        } else {
            console.log('User deletion cancelled.');
        }
    });
});
const tabLinks = document.querySelectorAll('.tab-link');
const tabContents = document.querySelectorAll('.tab-content');

tabLinks.forEach(link => {
    link.addEventListener('click', () => {
        // Get the target tab's ID from the data-tab attribute
        const tabId = link.getAttribute('data-tab');
        const targetTabContent = document.getElementById(tabId);

        // Remove 'active' class from all links and content panels
        tabLinks.forEach(l => l.classList.remove('active'));
        tabContents.forEach(c => c.classList.remove('active'));

        // Add 'active' class to the clicked link and its corresponding content
        link.classList.add('active');
        targetTabContent.classList.add('active');
    });
});