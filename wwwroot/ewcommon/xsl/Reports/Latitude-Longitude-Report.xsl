<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
				xmlns:ms="urn:schemas-microsoft-com:xslt">
	<xsl:import href="Report-Base.xsl"/>
	<xsl:import href="Formats/Report-Format-Loader.xsl"/>

	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>

	<!-- LatitudeLongitude download report hardcodes the order of columns -->
	<xsl:template match="Report" mode="reportHeaders">
		<xsl:variable name="latitudeLongitudeHeader">
			<Item>
				<SupplierID/>
				<Supplier_Name/>
				<nContactKey/>
				<cContactForeignRef/>
				<Longitude/>
				<Latitude/>
				<cContactCompany/>
				<cContactAddress/>
				<cContactCity/>
				<cContactState/>
				<cContactCountry/>
				<EditContact/>

			</Item>
		</xsl:variable>
		<xsl:apply-templates select="ms:node-set($latitudeLongitudeHeader)" mode="reportHeaderRow"/>
	</xsl:template>



	<!-- Latitude Longitude download actually makes each line item a row (not each order) -->
	<xsl:template match="Report" mode="reportRow">
		<xsl:apply-templates select="Item" mode="reportRow"/>
	</xsl:template>


	<!-- Latitude Longitude download determine the order of the cells-->
	<!-- ROW CELL CHOOSER -->
	<xsl:template match="Item" mode="reportRowCellFilter">

		<xsl:variable name="latitudeLongitudeItem">
			<Item>
				<SupplierID>
					<xsl:value-of select="SupplierID"/>
				</SupplierID>
				<Supplier_Name>
					<xsl:value-of select="Supplier_Name"/>
				</Supplier_Name>
				<nContactKey>
					<xsl:value-of select="nContactKey"/>
				</nContactKey>
				<cContactForeignRef>
					<xsl:value-of select="cContactForeignRef"/>
				</cContactForeignRef>
				<Longitude>
					<xsl:value-of select="Longitude"/>
				</Longitude>
				<Latitude>
					<xsl:value-of select="Latitude"/>
				</Latitude>
				<cContactCompany>
					<xsl:value-of select="cContactCompany"/>
				</cContactCompany>
				<cContactAddress>
					<xsl:value-of select="cContactAddress"/>
				</cContactAddress>
				<cContactCity>
					<xsl:value-of select="cContactCity"/>
				</cContactCity>
				<cContactState>
					<xsl:value-of select="cContactState"/>
				</cContactState>
				<cContactCountry>
					<xsl:value-of select="cContactCountry"/>
				</cContactCountry>

				<EditContact>
					<xsl:value-of select="EditContact"/>
				</EditContact>

			</Item>
		</xsl:variable>
		<xsl:apply-templates select="ms:node-set($latitudeLongitudeItem)/*/*" mode="reportCell"/>
	</xsl:template>


</xsl:stylesheet>
