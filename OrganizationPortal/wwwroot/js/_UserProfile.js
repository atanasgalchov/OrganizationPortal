(function () {

	_UserProfile = {
		init: function (args) {		
			
			var $container = $('#user-profile')
			var $form = $('#edit-user-form');
			
			$container
				.find('[data-action]')
					.click(function (e) {
						e.preventDefault();
						var $target = $(e.currentTarget);
						var action = $target.attr('data-action');

						var id = $form.find('[name="Id"]').val();

						if (action == 'back') {
							window.history.back();
						}
						else if (action == 'delete-image') {
							
							ASG.Ajax.post({
								url: args.deleteImaeUrl,
								data: { id: id },
								success(response) {
									if (response.isSuccess) {
										location.reload();
									}
								},
							});
						}
						else if (action == 'upload-image') {
							$form.find('[name="MainPicture"]').click();
						}
					})
			.end();

			var dataTable = $('#user-roles')
				.on('preXhr.dt', function (e, settings, data) {
					// start loading
					ASG.Html.showLoadingMask();
				})
				.on('xhr.dt', function (e, settings, json, xhr) {
					// done loading
					ASG.Html.hideLoadingMask();
				})
				.DataTable({
					processing: false,
					serverSide: true,
					ordering: false,
					paging: false,
					info: false,
					searching: false,
					ajax: {
						url: args.getRolesUrl,
						contentType: "application/json",
						type: "GET",
						dataType: "JSON"
					},
					dataSource: 'data',
					columns: [
						{
							"render": function (data, type, full, meta) {
								var checkedAttribute = args.model.UserRolesIds.filter(function (item) { return item == full.id })[0] ? "checked=checked" : "";
								return `<input type="checkbox" name="Roles" value="${full.id}" ${checkedAttribute} />`;
							}
						},
						{ data: 'name' }
					]
				});

			var dataTableNews = $('#user-profile-data-table-news').DataTable({
				info: false,
				searching: false,
				ordering: false,
				paging: false
			});

			$form
				.find('[name="MainPicture"]')
					.click(function (e) { this.value = null })
					.change(function (e) {
						
						if (this.files && this.files.length) {
							var file = this.files[0];
							var id = $form.find('[name="Id"]').val();

							var formData = new FormData();
							formData.append("image", file);
							formData.append("id", id);

							ASG.Ajax.post({
								url: args.saveImageUrl,
								data: formData,
								processData: false,
								contentType: false,
								success(response) {
									if (response.isSuccess) {
										location.reload();
										return;
									}
								},
							});
						}
					})
				.end();			

			$form
				.submit(function (e) {
					e.preventDefault();
					
					var $form = $(this);
					if (!$form.valid()) {
						return;
					}

					var data = $form.serializeToObject();
					data.UserRolesIds = $form.find('[name="Roles"]:checked').length > 0 ? $form.find('[name="Roles"]:checked').map(function (inx, el) { return $(el).val() }).get() : [];
					
					var settings = {
						url: $form.attr('action'),
						data: JSON.stringify(data),
						success(response) {
							if (response.isSuccess) {
								location.reload();
							}
						},
					};

					ASG.Ajax.postJson(settings);
			});
		}
	}

	return _UserProfile;
})()